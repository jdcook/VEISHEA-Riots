using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace Veishea
{
    public class PlayerController : Component
    {
        enum PlayerState
        {
            Punching,
            Normal,
            Vomiting,
        }
        PlayerState state = PlayerState.Normal;


        Entity physicalData;
        AnimationPlayer anims;
        public PlayerController(Game1 game, GameEntity entity)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            anims = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
        }

        private void PlayAnimation(string anim)
        {
            anims.StartClip(anim, MixType.None);
            curAnim = anim;
        }

        double punchCounter = 0;
        double punchLength = 780;

        public void Vomit()
        {
            physicalData.LinearVelocity = Vector3.Zero;
            PlayAnimation("k_puke");
            state = PlayerState.Vomiting;
            vomitCounter = anims.GetAniMillis("k_puke") - 20;
            vomitSoundCounter = 2000;
        }
        string curAnim = "k_idle1";
        float runspeed = 25;
        KeyboardState curkeys = Keyboard.GetState();
        KeyboardState prevkeys = Keyboard.GetState();
        MouseState curMouse = Mouse.GetState();
        MouseState prevMouse = Mouse.GetState();
        bool punched = false;
        double vomitCounter;
        double vomitSoundCounter;
        public override void Update(GameTime gameTime)
        {

            curkeys = Keyboard.GetState();
            curMouse = Mouse.GetState();
            if (state == PlayerState.Vomiting)
            {
                (Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent).SetEmitterVel("vomit", 60, "Bone_015");
                vomitCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (vomitCounter <= 0)
                {
                    state = PlayerState.Normal;
                    PlayAnimation("k_idle1");
                    (Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent).RemoveEmitter("vomit");
                }

                vomitSoundCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (vomitSoundCounter <= 0)
                {
                    (Game as Game1).VomitSound();
                    (Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent).AddEmitter(typeof(VomitSystem), "vomit", 200, 0, Vector3.Zero, "Bone_015");
                    vomitSoundCounter = double.MaxValue;
                }
            }
            else
            {

                punchCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (state == PlayerState.Punching)
                {
                    if (!punched && punchCounter >= punchLength / 2)
                    {
                        Game.PlayWhooshSound();
                        (Game.Services.GetService(typeof(EntityManager)) as EntityManager).CreatePunch(physicalData.Position, physicalData.OrientationMatrix.Forward, true);
                        punched = true;
                    }
                    else if (punchCounter >= punchLength)
                    {
                        state = PlayerState.Normal;
                    }
                }

                if (curkeys.IsKeyDown(Keys.W))
                {
                    Vector3 vel = physicalData.OrientationMatrix.Forward * runspeed;
                    vel.Y = 0;
                    physicalData.LinearVelocity = new Vector3(vel.X, physicalData.LinearVelocity.Y, vel.Z);
                }
                if (curkeys.IsKeyDown(Keys.D))
                {
                    Vector3 vel = physicalData.OrientationMatrix.Right * runspeed;
                    vel.Y = 0;
                    physicalData.LinearVelocity = new Vector3(vel.X, physicalData.LinearVelocity.Y, vel.Z);
                }
                if (curkeys.IsKeyDown(Keys.A))
                {
                    Vector3 vel = physicalData.OrientationMatrix.Left * runspeed;
                    vel.Y = 0;
                    physicalData.LinearVelocity = new Vector3(vel.X, physicalData.LinearVelocity.Y, vel.Z);
                }
                if (curkeys.IsKeyDown(Keys.S))
                {
                    Vector3 vel = physicalData.OrientationMatrix.Backward * runspeed;
                    vel.Y = 0;
                    physicalData.LinearVelocity = new Vector3(vel.X, physicalData.LinearVelocity.Y, vel.Z);
                }
                if (curkeys.IsKeyDown(Keys.Space) && prevkeys.IsKeyUp(Keys.Space) && physicalData.LinearVelocity.Y < 1)
                {
                    physicalData.LinearVelocity = physicalData.LinearVelocity + Vector3.Up * 55;
                }

                if (state == PlayerState.Normal && curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                {
                    PlayAnimation("k_punch");
                    state = PlayerState.Punching;
                    punchCounter = 0;
                    punched = false;
                }

                if (state == PlayerState.Normal)
                {
                    if (Math.Abs(physicalData.LinearVelocity.X) + Math.Abs(physicalData.LinearVelocity.Z) > .01f)
                    {
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
                }
            }

            prevkeys = curkeys;
            prevMouse = curMouse;
        }
    }
}
