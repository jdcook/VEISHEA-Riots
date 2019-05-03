using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Veishea
{
    public class GeneralComponentManager : GameComponent
    {
        List<Component> components = new List<Component>();
        public GeneralComponentManager(Game1 game)
            : base(game)
        {

        }

        public override void Update(GameTime gameTime)
        {
            for (int i = components.Count - 1; i >= 0; --i)
            {
                components[i].Update(gameTime);
                if (components[i].Remove)
                {
                    components[i].End();
                    components.RemoveAt(i);
                }
            }
        }

        public void AddComponent(Component c)
        {
            c.Start();
            components.Add(c);
        }
    }
}
