#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace SkinnedModelLib
{
    public enum MixType
    {
        None,
        MixOnce,
        MixInto,
        PauseAtEnd,
    }
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {
        #region Fields

        // Information about the currently playing animation clip.
        AnimationClip currentClipValue;
        TimeSpan currentTimeValue;
        int currentKeyframe;

        bool mixing = false;
        bool playMixedOnce = false;
        AnimationClip secondClipValue = null;
        TimeSpan secondTimeValue;
        int secondKeyframe;
        List<int> bonesToIgnore = null;
        TimeSpan mixDur = TimeSpan.Zero;

        // Current animation transform matrices.
        Matrix[] boneTransforms;
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;


        // Backlink to the bind pose and skeleton hierarchy data.
        public SkinningData skinningDataValue { get; private set; }


        #endregion

        public double GetAniMillis(string aniName)
        {
            return skinningDataValue.AnimationClips[aniName].Duration.TotalMilliseconds;
        }

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(SkinningData skinningData)
        {
            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            skinningDataValue = skinningData;

            boneTransforms = new Matrix[skinningData.BindPose.Count];
            worldTransforms = new Matrix[skinningData.BindPose.Count];
            skinTransforms = new Matrix[skinningData.BindPose.Count];

            PlaybackRate = 1f;
        }

        public void StopMixing()
        {
            mixing = false;
            secondClipValue = null;
        }
         /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(string clipName, MixType t)
        {
            StartClip(clipName, t, 1);
        }

        public void StartClip(string clipName, MixType t, float playbackRate)
        {
            this.PlaybackRate = playbackRate;
            pauseAtEnd = false;
            paused = false;
            mixing = false;
            secondClipValue = null;
            switch (t)
            {
                case MixType.None:
                    currentClipValue = skinningDataValue.AnimationClips[clipName];
                    currentTimeValue = TimeSpan.Zero;
                    currentKeyframe = 0;

                    // Initialize bone transforms to the bind pose.
                    skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
                    break;
                case MixType.MixOnce:
                    mixing = true;
                    playMixedOnce = true;
                    secondClipValue = skinningDataValue.AnimationClips[clipName];
                    mixDur = secondClipValue.Duration;
                    secondTimeValue = TimeSpan.Zero;
                    secondKeyframe = 0;
                    break;
                case MixType.MixInto:
                    playMixedOnce = false;
                    mixing = true;
                    secondClipValue = skinningDataValue.AnimationClips[clipName];
                    //takes the shortest: 1 second, time remaining in current animation, or half of the new animation's duration
                    mixDur = TimeSpan.FromMilliseconds(Math.Min(1000, Math.Min((currentClipValue.Duration - CurrentTime).TotalMilliseconds, secondClipValue.Duration.TotalMilliseconds / 2)));
                    secondTimeValue = TimeSpan.Zero;
                    secondKeyframe = 0;
                    bonesToIgnore = null;
                    break;
                case MixType.PauseAtEnd:
                    currentClipValue = skinningDataValue.AnimationClips[clipName];
                    currentTimeValue = TimeSpan.Zero;
                    currentKeyframe = 0;

                    // Initialize bone transforms to the bind pose.
                    skinningDataValue.BindPose.CopyTo(boneTransforms, 0);

                    pauseAtEnd = true;
                    break;
            }
        }

        bool pauseAtEnd = false;
        bool paused = false;
        public void PauseAnimation()
        {
            paused = true;
        }

        public void UnpauseAnimation()
        {
            paused = false;
        }

        public void SetNonMixedBones(List<int> boneIndicesToIgnore)
        {
            this.bonesToIgnore = boneIndicesToIgnore;
        }

        public float PlaybackRate { get; private set; }
        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(TimeSpan time, bool relativeToCurrentTime,
                           Matrix rootTransform)
        {
            time = TimeSpan.FromMilliseconds(time.TotalMilliseconds * PlaybackRate);
            UpdateBoneTransforms(time, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();

        }

        private void UpdateCurrentTime(TimeSpan time, bool relativeToCurrentTime)
        {
            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += currentTimeValue;

                // If we reached the end, loop back to the start.
                while (time >= currentClipValue.Duration)
                {
                    if (pauseAtEnd)
                    {
                        time = currentClipValue.Duration;
                        paused = true;
                        break;
                    }
                    else
                    {
                        time -= currentClipValue.Duration;
                    }
                }
            }

            if ((time < TimeSpan.Zero) || (time >= currentClipValue.Duration && !paused))
                throw new ArgumentOutOfRangeException("time");

            // If the position moved backwards, reset the keyframe index.
            if (time < currentTimeValue)
            {
                currentKeyframe = 0;
                skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
            }

            currentTimeValue = time;
        }

        private void UpdateSecondTime(TimeSpan time, bool relativeToCurrentTime)
        {
            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += secondTimeValue;

                // If we reached the end, stop mixing
                if(time >= secondClipValue.Duration || (!playMixedOnce && time >= mixDur))
                {
                    if (!playMixedOnce)
                    {
                        currentClipValue = secondClipValue;
                        currentTimeValue = TimeSpan.Zero;
                        currentKeyframe = 0;

                        // Initialize bone transforms to the bind pose.
                        skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
                        UpdateCurrentTime(time, relativeToCurrentTime);
                    }
                    StopMixing();
                    return;
                }
            }

            if ((time < TimeSpan.Zero) || (time >= secondClipValue.Duration))
                throw new ArgumentOutOfRangeException("time");

            // If the position moved backwards, reset the keyframe index.
            if (time < secondTimeValue)
            {
                secondKeyframe = 0;
                skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
            }

            secondTimeValue = time;
        }

        /// <summary>
        /// Helper used by the Update method to refresh the BoneTransforms data.
        /// </summary>
        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            if (currentClipValue == null)
            {
                throw new InvalidOperationException("AnimationPlayer.Update was called before StartClip");
            }

            if (mixing && secondClipValue == null)
            {
                throw new InvalidOperationException("AnimationPlayer.Update was supposed to mix two animations, but secondClipValue is null");
            }

            TimeSpan secondTime = time;

            if (!paused)
            {
                UpdateCurrentTime(time, relativeToCurrentTime);

                if (mixing)
                {
                    UpdateSecondTime(secondTime, relativeToCurrentTime);
                }
            }

            // Read keyframe matrices.
            IList<Keyframe> keyframes = currentClipValue.Keyframes;


            while (currentKeyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > currentTimeValue)
                    break;

                // Use this keyframe.
                Matrix transform = keyframe.Transform;
                boneTransforms[keyframe.Bone] = transform;
                currentKeyframe++;
            }

            if (mixing)
            {
                IList<Keyframe> secondKeyframes = secondClipValue.Keyframes;
                while (secondKeyframe < secondKeyframes.Count)
                {
                    Keyframe keyframe = secondKeyframes[secondKeyframe];

                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > secondTimeValue)
                        break;

                    // Use this keyframe.
                    Matrix transform = keyframe.Transform;
                    float dur = (float)(mixDur.Duration().TotalMilliseconds);
                    float cur = (float)(secondTimeValue.Duration().TotalMilliseconds);
                    float amt;
                    if (playMixedOnce)
                    {
                        amt = (float)((dur - cur) / dur);
                    }
                    else
                    {
                        amt = cur / dur;
                    }
                    bool alwayslerp = bonesToIgnore == null;
                    if (alwayslerp || !bonesToIgnore.Contains(keyframe.Bone))
                    {
                        boneTransforms[keyframe.Bone] = Matrix.Lerp(boneTransforms[keyframe.Bone], transform, amt);
                    }
                    secondKeyframe++;
                }
            }
        }


        /// <summary>
        /// Helper used by the Update method to refresh the WorldTransforms data.
        /// </summary>
        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] *
                                             worldTransforms[parentBone];
            }
        }


        /// <summary>
        /// Helper used by the Update method to refresh the SkinTransforms data.
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] *
                                            worldTransforms[bone];
            }
        }


        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] GetWorldTransforms()
        {
            return worldTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }


        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }


        /// <summary>
        /// Gets the current play position.
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }
    }
}
