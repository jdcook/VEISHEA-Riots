using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Veishea
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class AnimatedFireExplosionSystem : ParticleSystem
    {
        public AnimatedFireExplosionSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "explosion_animated";
            settings.framesPerSecond = 35;
            settings.totalFrames = 23;
            settings.SpriteDimensions = new Vector2(5, 5);

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(.65);

            settings.MinHorizontalVelocity = 40;
            settings.MaxHorizontalVelocity = 60;

            settings.MinVerticalVelocity = -40;
            settings.MaxVerticalVelocity = 40;

            settings.EndVelocity = 0;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 80;
            settings.MaxEndSize = 80;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
