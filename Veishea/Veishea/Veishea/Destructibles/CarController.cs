using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace Veishea
{
    public class CarController : DestructibleProp
    {
        public CarController(Game1 game, GameEntity entity)
            : base(game, entity)
        {
            stupid = 35;
        }

        protected override void Topple()
        {
            (Entity.GetSharedData(typeof(Entity)) as Entity).LinearVelocity += Vector3.Up * 105;
            Game.CarAlarm((Entity.GetSharedData(typeof(Entity)) as Entity).Position);
            (Entity.GetComponent(typeof(UnanimatedModelComponent)) as UnanimatedModelComponent).AddEmitter(typeof(SmokeSystem), "smoke", 15, 2, Vector3.Zero);
        }
    }
}
