using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veishea
{
    public class LightPoleController : DestructibleProp
    {
        public LightPoleController(Game1 game, GameEntity entity)
            : base(game, entity)
        {
            stupid = 25;
        }

        protected override void Topple()
        {
            Game.LightPoleTopple();
            base.Topple();
        }
    }
}
