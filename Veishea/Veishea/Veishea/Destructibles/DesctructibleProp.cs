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
    public enum PropState
    {
        Normal,
        KnockedOver,
    }
    public class DestructibleProp : Component
    {
        PropState state = PropState.Normal;

        protected int stupid = 40;
        Entity physicalData;
        public DestructibleProp(Game1 game, GameEntity entity)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }

        protected virtual void Topple()
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (state == PropState.Normal && Vector3.Dot(physicalData.OrientationMatrix.Up, Vector3.Up) < .25f)
            {
                state = PropState.KnockedOver;
                Game.ToppleProp(stupid);
                Topple();
            }
            base.Update(gameTime);
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity ge = other.Tag as GameEntity;
            if (ge != null && ge.Name == "punch")
            {
                Game.PunchProp();
                Entity.Dead = true;
            }
        }
    }
}
