using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Veishea
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class VomitSystem : ParticleSystem
    {
        public VomitSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "puke";

            settings.MaxParticles = 4000;

            settings.Duration = TimeSpan.FromSeconds(.25);

            settings.MinHorizontalVelocity = -25;
            settings.MaxHorizontalVelocity = 25;

            settings.MinVerticalVelocity = -25;
            settings.MaxVerticalVelocity = 25;

            settings.Gravity = new Vector3(0, -50, 0);

            settings.EndVelocity = 0;

            settings.MinRotateSpeed = -8;
            settings.MaxRotateSpeed = 8;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 4;
            settings.MaxEndSize = 4;

            settings.BlendState = BlendState.Additive;
        }
    }
}
