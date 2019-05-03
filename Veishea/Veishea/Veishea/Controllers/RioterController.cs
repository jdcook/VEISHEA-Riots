using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace Veishea
{
    public enum RioterState
    {
        Idling,
        Following,
        Attacking,
        Vomiting,
    }
    public class RioterController : Component
    {
        RioterState state = RioterState.Idling;
        Entity physicalData;
        AnimationPlayer anims;
        public RioterController(Game1 game, GameEntity entity)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            anims = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            playerData = game.GetPlayerEntity();
            runSpeed = game.rand.Next(15, 28);
        }

        string curAnim = "k_idle1";
        private void PlayAnimation(string anim)
        {
            anims.StartClip(anim, MixType.None);
            curAnim = anim;
        }

        Entity attackingObj;
        public void AttackObject(GameEntity ob)
        {
            attackingObj = ob.GetSharedData(typeof(Entity)) as Entity;
        }

        public void Vomit()
        {
            if (state != RioterState.Vomiting)
            {
                physicalData.LinearVelocity = Vector3.Zero;
                PlayAnimation("k_puke");
                state = RioterState.Vomiting;
                vomitCounter = anims.GetAniMillis("k_puke") - 20;
                vomitSoundCounter = 2000;
            }
        }

        float runSpeed;
        Entity playerData;
        double jumpCounter;
        double punchCounter;
        double nextPunchCounter;
        double stopPunchCounter;
        bool punched = false;
        double vomitCounter;
        double vomitSoundCounter;
        public override void Update(GameTime gameTime)
        {
            Vector3 diff;
            switch (state)
            {
                case RioterState.Idling:
                    if (curAnim != "k_idle1")
                    {
                        PlayAnimation("k_idle1");
                    }
                    break;
                case RioterState.Following:
                    if (attackingObj != null)
                    {
                        state = RioterState.Attacking;
                    }
                    diff = playerData.Position - physicalData.Position;
                    if (Math.Abs(diff.X) + Math.Abs(diff.Z) > 15)
                    {
                        newDir = GetPhysicsYaw(diff);
                        AdjustDir(runSpeed, .12f);
                        if (curAnim != "k_walk")
                        {
                            PlayAnimation("k_walk");
                        }
                    }
                    else
                    {
                        if (curAnim != "k_idle1")
                        {
                            PlayAnimation("k_idle1");
                        }
                    }
                    break;
                case RioterState.Attacking:
                    diff = attackingObj.Position - physicalData.Position;
                    newDir = GetPhysicsYaw(diff);
                    AdjustDir(runSpeed, .18f);

                    jumpCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (jumpCounter <= 0)
                    {
                        physicalData.LinearVelocity += Vector3.Up * 55;
                        jumpCounter = (Game as Game1).rand.Next(1000, 2500);
                    }

                    nextPunchCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (nextPunchCounter <= 0)
                    {
                        PlayAnimation("k_punch");
                        nextPunchCounter = (Game as Game1).rand.Next(1000, 3000);
                        punched = false;
                        punchCounter = 315;
                        stopPunchCounter = 780;
                    }

                    stopPunchCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (stopPunchCounter <= 0)
                    {
                        PlayAnimation("k_walk");
                        stopPunchCounter = 5000;
                    }

                    punchCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (!punched && punchCounter <= 0)
                    {
                        Game.PlayWhooshSound();
                        (Game.Services.GetService(typeof(EntityManager)) as EntityManager).CreatePunch(physicalData.Position, physicalData.OrientationMatrix.Forward, false);
                        punched = true;
                        
                    }
                    break;
                case RioterState.Vomiting:
                    (Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent).SetEmitterVel("vomit", 60, "Bone_015");
                    vomitCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (vomitCounter <= 0)
                    {
                        state = RioterState.Following;
                        PlayAnimation("k_idle1");
                        (Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent).RemoveEmitter("vomit");
                    }

                    vomitSoundCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (vomitSoundCounter <= 0)
                    {
                        (Game as Game1).SoftVomitSound();
                        (Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent).AddEmitter(typeof(VomitSystem), "vomit", 150, 0, Vector3.Zero, "Bone_015");
                        vomitSoundCounter = double.MaxValue;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        public void MakePlayerMinion()
        {
            state = RioterState.Following;
        }

        float curDir;
        float newDir;
        protected void AdjustDir(float runSpeed, float turnSpeed)
        {
            if (curDir != newDir)
            {
                if (Math.Abs(curDir - newDir) <= turnSpeed * 1.3f)
                {
                    curDir = newDir;
                    Vector3 newVel = new Vector3((float)Math.Cos(curDir), 0, (float)Math.Sin(curDir));
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(newVel), 0, 0);
                    newVel *= runSpeed;
                    newVel.Y = physicalData.LinearVelocity.Y;
                    physicalData.LinearVelocity = newVel;
                }
                else
                {
                    float add = turnSpeed;
                    float diff = curDir - newDir;
                    if (diff > 0 && diff < MathHelper.Pi || diff < 0 && -diff > MathHelper.Pi)
                    {
                        add *= -1;
                    }
                    curDir += add;
                    if (curDir > MathHelper.TwoPi)
                    {
                        curDir -= MathHelper.TwoPi;
                    }
                    else if (curDir < 0)
                    {
                        curDir += MathHelper.TwoPi;
                    }
                    Vector3 newVel = new Vector3((float)Math.Cos(curDir), 0, (float)Math.Sin(curDir));
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(newVel), 0, 0);
                    newVel *= runSpeed;
                    newVel.Y = physicalData.LinearVelocity.Y;
                    physicalData.LinearVelocity = newVel;
                }
            }
        }

        protected float GetGraphicsYaw(Vector3 move)
        {
            Vector3 lmove = new Vector3();
            lmove.X = move.X;
            lmove.Y = move.Y;
            lmove.Z = move.Z;
            if (lmove.Z == 0)
            {
                lmove.Z = .00000001f;
            }
            else
            {
                lmove.Normalize();
            }
            float yaw = (float)Math.Atan(lmove.X / lmove.Z);
            if (lmove.Z < 0 && lmove.X >= 0
                || lmove.Z < 0 && lmove.X < 0)
            {
                yaw += MathHelper.Pi;
            }
            yaw += MathHelper.Pi;
            return yaw;
        }

        protected float GetPhysicsYaw(Vector3 move)
        {
            float retYaw = -GetGraphicsYaw(move) - MathHelper.PiOver2;
            while (retYaw > MathHelper.TwoPi)
            {
                retYaw -= MathHelper.TwoPi;
            }
            while (retYaw < 0)
            {
                retYaw += MathHelper.TwoPi;
            }

            return retYaw;
        }
    }
}
