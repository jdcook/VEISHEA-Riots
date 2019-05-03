using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModelLib;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;
using Microsoft.Xna.Framework.Input;

namespace Veishea
{
    public class SharedGraphicsParams
    {
        public float alpha;
        public float lineIntensity;
        public Color lineColor;
        public Vector3 size;
        public SharedGraphicsParams()
        {
            alpha = 1f;
            lineIntensity = 1f;
            size = new Vector3(1);
            lineColor = Color.Black;
        }
    }

    public class AnimatedModelComponent : DrawableComponent3D
    {
        //components
        protected AnimationPlayer animationPlayer;

        //fields
        protected Model model;
        protected Vector3 localOffset = Vector3.Zero;
        protected Dictionary<string, Model> syncedModels;
        protected Matrix yawOffset = Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);

        protected SharedGraphicsParams modelParams;

        public AnimatedModelComponent(Game1 game, GameEntity entity, Model model, float drawScale, Vector3 drawOffset)
            : base(game, entity)
        {
            this.model = model;
            this.localOffset = drawOffset;
            this.syncedModels = entity.GetSharedData(typeof(Dictionary<string, Model>)) as Dictionary<string, Model>;
            this.animationPlayer = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;

            animationPlayer.StartClip(animationPlayer.skinningDataValue.AnimationClips.Keys.First(), MixType.None);

            modelParams = new SharedGraphicsParams();
            modelParams.size = new Vector3(drawScale);
            entity.AddSharedData(typeof(SharedGraphicsParams), modelParams);

        }

        public void SetEmitterUp(string key, float amount)
        {
            if (emitters.ContainsKey(key))
            {
                emitters[key].SetAlongUpAmount(amount);
            }
        }

        public void SetEmitterVel(string emitterName, float vel, string relativeBone)
        {
            ParticleEmitter possEmitter;
            if (emitters.TryGetValue(emitterName, out possEmitter))
            {
                if (possEmitter != null)
                {
                    possEmitter.SetVelocity(vel * (animationPlayer.GetWorldTransforms()[model.Bones[relativeBone].Index - 2].Forward));
                }
            }
        }

        public ParticleEmitter AddEmitter(Type particleType, string systemName, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter, string attachBoneName)
        {
            return AddEmitter(particleType, systemName, particlesPerSecond, maxOffset, offsetFromCenter, model.Bones[attachBoneName].Index - 2);
        }

        public ParticleEmitter AddEmitter(Type particleType, string systemName, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter, int attachIndex)
        {
            return AddEmitter(particleType, systemName, particlesPerSecond, maxOffset, maxOffset, offsetFromCenter, attachIndex);
        }

        public ParticleEmitter AddEmitter(Type particleType, string systemName, float particlesPerSecond, int maxHorizontalOffset, int maxVerticalOffset, Vector3 offsetFromCenter, int attachIndex)
        {
            ParticleEmitter toAdd = new ParticleEmitter((Game.Services.GetService(typeof(CameraComponent)) as CameraComponent), (Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(particleType), particlesPerSecond, physicalData.Position, offsetFromCenter, attachIndex);
            toAdd.SetHorizontalOffset(maxHorizontalOffset);
            toAdd.SetVerticalOffset(maxVerticalOffset);
            if (!emitters.ContainsKey(systemName))
            {
                emitters.Add(systemName, toAdd);
            }
            else
            {
                emitters[systemName] = toAdd;
            }

            return toAdd;
        }

        protected Vector3 vLightDirection = new Vector3(-1.0f, -.5f, 1.0f);
        Matrix rot = Matrix.Identity;
        public override void Update(GameTime gameTime)
        {
            //need to do this conversion from Matrix3x3 to Matrix; Matrix3x3 is just a bepu thing
            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            //either do this or Matrix.CreateFromQuaternion(physicalData.Orientation);
            //this is probably faster? not sure how CreateFromQuaternion works
            rot = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);
            rot *= yawOffset;

            Matrix conglomeration = Matrix.CreateScale(modelParams.size);
            conglomeration *= Matrix.CreateTranslation(localOffset);
            conglomeration *= rot;
            conglomeration *= Matrix.CreateTranslation(physicalData.Position);

            animationPlayer.Update(gameTime.ElapsedGameTime, true, conglomeration);

            Matrix[] boneTransforms = animationPlayer.GetWorldTransforms();

            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, ParticleEmitter> k in emitters)
            {
                if (k.Value.BoneIndex < 0)
                {
                    k.Value.Update(gameTime, physicalData.Position, rot);
                }
                else
                {
                    Vector3 bonePos = boneTransforms[k.Value.BoneIndex].Translation;
                    k.Value.Update(gameTime, bonePos, rot);

                    k.Value.SetUpTranslationVector(boneTransforms[k.Value.BoneIndex].Up);
                }

                if (k.Value.Dead)
                {
                    toRemove.Add(k.Key);
                }
            }

            for (int i = toRemove.Count - 1; i >= 0; --i)
            {
                emitters.Remove(toRemove[i]);
            }
        }

        public override void Draw(GameTime gameTime, CameraComponent camera)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            Matrix view = camera.View;
            Matrix projection = camera.Projection;

                //drawing with toon shader
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (CustomSkinnedEffect effect in mesh.Effects)
                    {
                        effect.SetBoneTransforms(bones);

                        effect.LightPositions = camera.lightPositions;
                        

                        effect.View = view;
                        effect.Projection = projection;
                    }

                    mesh.Draw();
                }
            }
        public AnimationClip GetAnimationClip(string clipName)
        {
            return animationPlayer.skinningDataValue.AnimationClips[clipName];
        }

        public Vector3 GetBonePosition(string boneName)
        {
            return animationPlayer.GetWorldTransforms()[model.Bones[boneName].Index - 2].Translation;
        }

    }
}
