using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Veishea
{
    class ParticleManager : GameComponent
    {
        Dictionary<Type, ParticleSystem> systems = new Dictionary<Type, ParticleSystem>();
        CameraComponent camera;
        public ParticleManager(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            systems.Add(typeof(AnimatedFireExplosionSystem), new AnimatedFireExplosionSystem(Game, Game.Content));
            systems.Add(typeof(BeerGlowSystem), new BeerGlowSystem(Game, Game.Content));
            systems.Add(typeof(BeerExplosion), new BeerExplosion(Game, Game.Content));
            systems.Add(typeof(VomitSystem), new VomitSystem(Game, Game.Content));
            systems.Add(typeof(SmokeSystem), new SmokeSystem(Game, Game.Content));
            

            foreach (KeyValuePair<Type, ParticleSystem> k in systems)
            {
                k.Value.Initialize();
            }
        }

        public ParticleSystem GetSystem(Type t)
        {
            if (!systems[t].InitializedFlag)
            {
                systems[t].Initialize();
            }
            return systems[t];
        }

        public override void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, ParticleSystem> k in systems)
            {
                k.Value.Update(gameTime);
                k.Value.SetCamera(camera.View, camera.Projection);
            }
        }

        public void Draw(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, ParticleSystem> k in systems)
            {
                k.Value.Draw(gameTime);
            }
        }
    }
}
