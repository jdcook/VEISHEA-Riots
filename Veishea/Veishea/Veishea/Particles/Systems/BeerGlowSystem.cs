using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Veishea
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class BeerGlowSystem : ParticleSystem
    {
        public BeerGlowSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(1.25);

            settings.StartColor = new Color(255, 204, 38f);
            settings.EndColor = new Color(255, 204, 38f);

            settings.StartColor = Color.Gold * .65f;
            settings.EndColor= Color.Gold * .65f;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = -15;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;

            settings.BlendState = BlendState.Additive;
        }
    }
}
