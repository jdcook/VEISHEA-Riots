using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;


namespace Veishea
{
    public class PunchController : Component
    {
        Entity physicalData;
        double duration = 100;
        bool player;
        public PunchController(Game1 game, GameEntity entity, bool player)
            : base(game, entity)
        {
            this.player = player;
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }

        public override void Update(GameTime gameTime)
        {
            duration -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (duration <= 0)
            {
                Entity.KillEntity();
            }
            base.Update(gameTime);
        }


        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity ge = other.Tag as GameEntity;
            if (ge != null && ge.Name != "player" && ge.Name != "derper")
            {
                if (ge.Name == "prop")
                {
                    if (player)
                    {
                        Game.PunchObject(ge);
                    }
                    else
                    {
                        Game.AddStupid(0);
                    }
                }
                Entity.KillEntity();
            }
        }
    }
}
