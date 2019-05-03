using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysicsDrawer;
using BEPUphysicsDrawer.Lines;
using BEPUphysics.CollisionTests;
using BEPUphysics.CollisionRuleManagement;

namespace Veishea
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public CollisionGroup guyCollisionGroup;
        public CollisionGroup punchCollisionGroup;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ModelManager modelManager;
        GeneralComponentManager genComponentManager;
        EntityManager entities;
        CameraComponent camera;
        BoundingBoxDrawer modelDrawer;
        ParticleManager particles;

        public Entity GetPlayerEntity()
        {
            return camera.physicalData;
        }

        string[] stupidStrings = new string[8] {
            "Feeling a Little Stupid",
            "Actually Stupid",
             "Very Stupid",
             "Complete Moron",
             "Lacking Basic Human Decency",
             "You just made 500 people lose faith in humanity",
             "WHYYYYYYYYYYYYYYYYYYYYYYYYYYY",
             "YOU ARE WHY VEISHEA WAS CANCELLED"};
        int stupidity = 0;
        int maxStupid = 1300;
        int beer = 0;
        int beerThreshhold = 3;
        public void DrinkBeer(Vector3 pos)
        {
            beerSound.Play();
            ParticleSystem expl = particles.GetSystem(typeof(BeerExplosion));
            for (int i = 0; i < 25; ++i)
            {
                expl.AddParticle(pos, Vector3.Zero);
            }
            ++beer;
            if (beer >= beerThreshhold)
            {
                beerThreshhold = 2;
                entities.Puke();
                beer = 0;
            }
            AddStupid(50);
        }
        public void VomitSound()
        {
            vomitSound.Play();
        }
        public void SoftVomitSound()
        {
            vomitSound.Play(.5f, 0, (float)(rand.Next(21) - 10) / 10.0f);
        }
        public void PunchProp()
        {
            PlayPunchSound();
        }
        public void ToppleProp(int stupid)
        {
            AddStupid(stupid);
        }

        int nextLevelOfStupid = 20;
        public void AddStupid(int stupid)
        {
            if (stupidity <= 0)
            {
                MediaPlayer.Play(derpsong);
                songDuration = derpsong.Duration.TotalMilliseconds;
            }
            stupidity += stupid;

            if (stupidity > nextLevelOfStupid)
            {
                entities.RecruitNextRioter();
                nextLevelOfStupid = Math.Min(maxStupid, entities.recruits.Count * 5 + 20);
            }
            if (stupidity > maxStupid)
            {
                stupidity = maxStupid;
            }
        }

        
        public Random rand;
        Space physics;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            physics = new Space();

            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < 10 * Environment.ProcessorCount; ++i)
                {
                    physics.ThreadManager.AddThread();
                }
            }
            physics.ForceUpdater.Gravity = new Vector3(0, -140, 0);
            physics.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            Services.AddService(typeof(Space), physics);

            rand = new Random();

            guyCollisionGroup = new CollisionGroup();
            punchCollisionGroup = new CollisionGroup();
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(punchCollisionGroup, guyCollisionGroup), CollisionRule.NoSolver);

        }

        #region Sounds
        SoundEffect[] punches = new SoundEffect[4];
        SoundEffect[] whooshes = new SoundEffect[4];
        SoundEffect carAlarm;
        SoundEffect explosion;
        SoundEffect[] shouts = new SoundEffect[3];
        SoundEffect lightPoleCrash;
        SoundEffect mailboxCrash;
        SoundEffect beerSound;
        SoundEffect vomitSound;

        public void PlayPunchSound()
        {
            punches[rand.Next(4)].Play(.5f, 0, 0);
        }
        public void PlayWhooshSound()
        {
            whooshes[rand.Next(4)].Play(.25f, 0, 0);
        }
        public void LightPoleTopple()
        {
            lightPoleCrash.Play();
        }
        public void MailboxCrash()
        {
            mailboxCrash.Play();
        }
        public void CarAlarm(Vector3 pos)
        {
            ParticleSystem expl = particles.GetSystem(typeof(AnimatedFireExplosionSystem));
            for (int i = 0; i < 25; ++i)
            {
                expl.AddParticle(pos, Vector3.Zero);
            }
            carAlarm.Play(.2f, 0, 0);
            explosion.Play();
        }
        #endregion

        public void PunchObject(GameEntity obj)
        {
            entities.AttractMinions(obj);
            AddStupid(2);
        }

        float average = 1;
        protected override void Initialize()
        {
            int height = 600;
            int width = 1000;

            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();



            float maxX = GraphicsDevice.Viewport.Width;
            float maxY = GraphicsDevice.Viewport.Height;
            float xRatio = maxX / 600;
            float yRatio = maxY / 1000;
            average = (xRatio + yRatio) / 2;


            camera = new CameraComponent(this);
            Components.Add(camera);
            Services.AddService(typeof(CameraComponent), camera);

            modelManager = new ModelManager(this);
            Components.Add(modelManager);
            Services.AddService(typeof(ModelManager), modelManager);

            genComponentManager = new GeneralComponentManager(this);
            Components.Add(genComponentManager);
            Services.AddService(typeof(GeneralComponentManager), genComponentManager);

            entities = new EntityManager(this);
            Components.Add(entities);
            Services.AddService(typeof(EntityManager), entities);

            particles = new ParticleManager(this);
            Components.Add(particles);
            Services.AddService(typeof(ParticleManager), particles);

            modelDrawer = new BoundingBoxDrawer(this);

            base.Initialize();
        }

        Song derpsong;
        BasicEffect effectModelDrawer;
        protected override void LoadContent()
        {


            font = Content.Load<SpriteFont>("font");
            white = Content.Load<Texture2D>("whitePixel");
            effectModelDrawer = new BasicEffect(GraphicsDevice);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            entities.CreateLevel();
            entities.CreatePlayer();
            entities.CreateMobs();

            derpsong = Content.Load<Song>("derpsong");
            MediaPlayer.Volume = .35f;


            for (int i = 0; i < 4; ++i)
            {
                punches[i] = Content.Load<SoundEffect>("Sounds\\sword_hit" + i);
            }
            for (int i = 0; i < 4; ++i)
            {
                whooshes[i] = Content.Load<SoundEffect>("Sounds\\whoosh" + i);
            }
            for (int i = 0; i < 3; ++i)
            {
                shouts[i] = Content.Load<SoundEffect>("Sounds\\VIESHEA" + (i + 1));
            }
            carAlarm = Content.Load<SoundEffect>("Sounds\\carAlarm");
            explosion = Content.Load<SoundEffect>("Sounds\\explosion");
            lightPoleCrash = Content.Load<SoundEffect>("Sounds\\Lightpole");
            mailboxCrash = Content.Load<SoundEffect>("Sounds\\mailbox");
            beerSound = Content.Load<SoundEffect>("Sounds\\gulp");
            vomitSound = Content.Load<SoundEffect>("Sounds\\puke");
        }

        protected override void UnloadContent()
        {

        }

        double randomShoutCounter = 4000;
        double songDuration = float.MaxValue;

        KeyboardState curKeys = Keyboard.GetState();
        KeyboardState prevKeys = Keyboard.GetState();
        protected override void Update(GameTime gameTime)
        {
            songDuration -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (songDuration <= 0)
            {
                MediaPlayer.Play(derpsong);
                songDuration = derpsong.Duration.TotalMilliseconds;
            }

            randomShoutCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (randomShoutCounter <= 0)
            {
                shouts[rand.Next(3)].Play(.5f, 0, 0);
                randomShoutCounter = Math.Max(500, rand.Next(6000, 8000) * (1 - (float)stupidity / (float)maxStupid));
            }

            physics.Update();

            curKeys = Keyboard.GetState();

            if (curKeys.IsKeyDown(Keys.Escape) && prevKeys.IsKeyUp(Keys.Escape))
            {
                menu = !menu;
            }

            if (menu && curKeys.IsKeyDown(Keys.Enter) && prevKeys.IsKeyUp(Keys.Enter))
            {
                Exit();
            }

            if (menu && curKeys.IsKeyDown(Keys.Tab) && prevKeys.IsKeyUp(Keys.Tab))
            {
                fullScreen = !fullScreen;
                if (fullScreen)
                {
                    int height = 600;
                    int width = 1000;

                    graphics.PreferredBackBufferWidth = width;
                    graphics.PreferredBackBufferHeight = height;
                    graphics.IsFullScreen = false;
                    graphics.ApplyChanges();



                    float maxX = GraphicsDevice.Viewport.Width;
                    float maxY = GraphicsDevice.Viewport.Height;
                    float xRatio = maxX / 600;
                    float yRatio = maxY / 1000;
                    average = (xRatio + yRatio) / 2;
                }
                else
                {
                    graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    graphics.IsFullScreen = true;
                    graphics.ApplyChanges();



                    float maxX = GraphicsDevice.Viewport.Width;
                    float maxY = GraphicsDevice.Viewport.Height;
                    float xRatio = maxX / 600;
                    float yRatio = maxY / 1000;
                    average = (xRatio + yRatio) / 2;
                }
            }


            prevKeys = curKeys;
            base.Update(gameTime);
        }

        SpriteFont font;
        Texture2D white;
        bool menu = false;
        bool fullScreen = false;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            modelManager.Draw(gameTime);
            particles.Draw(gameTime);


            base.Draw(gameTime);
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Menu: ESC", new Vector2(5 * average, 5 * average), Color.Yellow, 0, Vector2.Zero, average, SpriteEffects.None, 0);

            spriteBatch.Draw(white, new Rectangle((int)(50 * average), (int)(75 * average), (int)(30 * average), (int)(300 * average)), Color.White * .5f);
            spriteBatch.Draw(white, new Rectangle((int)(50 * average), (int)(75 * average), (int)(30 * average), (int)(300 * average * (float)stupidity / (float)maxStupid)), Color.Blue);
            spriteBatch.DrawString(font, "Stupid-O-Meter", new Vector2(50 * average, 50 * average), Color.DarkRed, 0, Vector2.Zero, average, SpriteEffects.None, 0);

            int stupidIndex = Math.Min(stupidStrings.Length - 1, (int)((float)stupidity * (float)(stupidStrings.Length - 1) / (float)(maxStupid)));
            spriteBatch.DrawString(font, stupidStrings[stupidIndex], new Vector2(85 * average, 75 * average + 300 * average * (float)stupidity / (float)maxStupid), Color.Lerp(Color.Red, Color.Green, 1 - ((float)stupidIndex / maxStupid)), 0, Vector2.Zero, average, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, "Rioters: " + entities.recruits.Count, new Vector2(10 * average, GraphicsDevice.Viewport.Height - 60 * average), Color.Red, 0, Vector2.Zero, average, SpriteEffects.None, 0);

            if (menu)
            {
                spriteBatch.DrawString(font, "ESCAPE to resume\n\nENTER to exit\n\nTAB to toggle fullscreen\n\nLEFT MOUSE to punch\nSPACEBAR to jump\nMOUSE SCROLL to zoom", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.Yellow, 0, Vector2.Zero, average, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Derp Song: Artix Entertainment LLC", new Vector2(GraphicsDevice.Viewport.Width / 2 - 20 * average, 5 * average), Color.Yellow, 0, Vector2.Zero, average, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }
    }
}
