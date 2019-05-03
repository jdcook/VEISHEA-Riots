using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Veishea
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class SmokeSystem : ParticleSystem
    {
        public SmokeSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(5);
            settings.MinVerticalVelocity = 5;
            settings.MaxVerticalVelocity = 10;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 15;

            settings.MinEndSize = 55;
            settings.MaxEndSize = 70;

            settings.BlendState = BlendState.Additive;
        }
    }
}
