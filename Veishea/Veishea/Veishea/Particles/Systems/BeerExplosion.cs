using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Veishea
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class BeerExplosion : ParticleSystem
    {
        public BeerExplosion(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(.65f);

            settings.StartColor = new Color(255, 204, 38f);
            settings.EndColor = new Color(255, 204, 38f);

            settings.MinHorizontalVelocity = 40;
            settings.MaxHorizontalVelocity = 60;

            settings.MinVerticalVelocity = -40;
            settings.MaxVerticalVelocity = 40;

            settings.EndVelocity = 0;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 30;
            settings.MaxEndSize = 30;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
