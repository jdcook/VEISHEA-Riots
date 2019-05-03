using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veishea
{
    public class MailboxController : DestructibleProp
    {
        public MailboxController(Game1 game, GameEntity entity)
            : base(game, entity)
        {
            stupid = 10;
        }

        protected override void Topple()
        {
            Game.LightPoleTopple();
            base.Topple();
        }
    }
}
