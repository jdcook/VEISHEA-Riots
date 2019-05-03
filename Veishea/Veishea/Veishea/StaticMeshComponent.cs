using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Collidables;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionRuleManagement;


namespace Veishea
{
    public class StaticMeshComponent : Component
    {
        protected StaticMesh collidable;
        public StaticMeshComponent(Game1 game, GameEntity entity)
            : base(game, entity)
        {
            this.collidable = Entity.GetSharedData(typeof(StaticMesh)) as StaticMesh;
        }

        public override void Start()
        {
            collidable.Tag = Entity;
            (Game.Services.GetService(typeof(Space)) as Space).Add(collidable);

            base.Start();
        }

        public override void Update(GameTime gametime)
        {

        }

        public override void End()
        {
            (Game.Services.GetService(typeof(Space)) as Space).Remove(collidable);
        }
    }
}
