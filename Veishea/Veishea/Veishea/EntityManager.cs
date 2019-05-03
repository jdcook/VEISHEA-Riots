using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Collidables;
using BEPUphysics.DataStructures;
using SkinnedModelLib;

namespace Veishea
{
    public class EntityManager : GameComponent
    {
        protected ModelManager modelManager;
        protected GeneralComponentManager genComponentManager;

        protected CameraComponent camera;

        protected Game1 mainGame;

        public EntityManager(Game1 game)
            : base(game)
        {
            this.mainGame = game;
            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
        }

        #region Loading
        public override void Initialize()
        {
            base.Initialize();
            modelManager = Game.Services.GetService(typeof(ModelManager)) as ModelManager;
            genComponentManager = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            toonAnimatedEffect = Game.Content.Load<Effect>("Shaders\\ToonSkinnedEffect");
            effectCellShading = Game.Content.Load<Effect>("Shaders\\CellShader");

        }


        protected Effect toonAnimatedEffect;
        protected Effect effectCellShading;
        protected Dictionary<string, Model> animatedModels = new Dictionary<string, Model>();
        protected Dictionary<string, Model> unanimatedModels = new Dictionary<string, Model>();

        public Model GetAnimatedModel(string filePath)
        {
            Model m;
            if (animatedModels.TryGetValue(filePath, out m))
            {
                return m;
            }
            else
            {
                LoadAnimatedModel(out m, filePath);
                animatedModels.Add(filePath, m);
                return m;
            }
        }

        public Model GetUnanimatedModel(string modelPath)
        {
            Model m;
            if (unanimatedModels.TryGetValue(modelPath, out m))
            {
                return m;
            }
            else
            {
                LoadUnanimatedModel(out m, modelPath);
                unanimatedModels.Add(modelPath, m);
                return m;
            }
        }

        public void LoadAnimatedModel(out Model model, string filePath)
        {
            model = Game.Content.Load<Model>(filePath);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    SkinnedEffect skinnedEffect = part.Effect as SkinnedEffect;
                    if (skinnedEffect != null)
                    {   
                        CustomSkinnedEffect newEffect = new CustomSkinnedEffect(toonAnimatedEffect);
                        newEffect.CopyFromSkinnedEffect(skinnedEffect);
                        newEffect.LightPositions = camera.lightPositions;
                        part.Effect = newEffect;
                    }
                }
            }
        }

        public void LoadUnanimatedModel(out Model model, string modelPath)
        {
            model = Game.Content.Load<Model>(modelPath);

            //if this model has already been loaded, don't process its textures again
            if (model.Meshes[0].Effects[0] is BasicEffect)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        BasicEffect oldEffect = part.Effect as BasicEffect;

                        Effect newEffect = effectCellShading.Clone();
                        newEffect.Parameters["ColorMap"].SetValue(oldEffect.Texture);
                        newEffect.Parameters["lightPositions"].SetValue(camera.lightPositions);
                        part.Effect = newEffect;
                    }
                }
            }
        }
        #endregion

        double nextRecruitVomitTime = 0;
        public override void Update(GameTime gameTime)
        {
            if (timesVomited > 0)
            {
                nextRecruitVomitTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (nextRecruitVomitTime <= 0)
                {
                    (recruits[(Game as Game1).rand.Next(recruits.Count)].GetComponent(typeof(RioterController)) as RioterController).Vomit();
                    nextRecruitVomitTime = Math.Max(500, (float)(Game as Game1).rand.Next(2000, 4000) * (1.0f - (float)recruits.Count / 140.0f));
                }
            }
            base.Update(gameTime);
        }

        Vector3 playerSpawn = new Vector3(500, 10, 0);
        GameEntity player;
        int timesVomited = 0;
        public void Puke()
        {
            ++timesVomited;
            (player.GetComponent(typeof(PlayerController)) as PlayerController).Vomit();
        }
        public void CreatePlayer()
        {
            GameEntity entity = new GameEntity("player");

            Entity box = new Box(playerSpawn, 2f, 4, 2f, 8);
            box.CollisionInformation.CollisionRules.Group = (Game as Game1).guyCollisionGroup;
            box.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            box.LocalInertiaTensorInverse = new Matrix3X3();
            entity.AddSharedData(typeof(Entity), box);
            camera.AssignEntity(box);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            Model playerModel = GetAnimatedModel("Models\\Player\\k_idle1");
            AnimationPlayer anims = new AnimationPlayer(playerModel.Tag as SkinningData);
            entity.AddSharedData(typeof(AnimationPlayer), anims);

            AnimatedModelComponent graphics = new AnimatedModelComponent(mainGame, entity, playerModel, 1, Vector3.Down * 2);
            entity.AddComponent(typeof(AnimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            PlayerController controller = new PlayerController(mainGame, entity);
            entity.AddComponent(typeof(PlayerController), controller);
            genComponentManager.AddComponent(controller);

            player = entity;
        }

        public void CreateMobs()
        {
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    CreateRioter(new Vector3(i * 100 - 500, 10, j * 4 - 20));
                    CreateRioter(new Vector3(i * 100 - 495, 10, j * 4 - 20));

                    CreateRioter(new Vector3(i * 100 - 500, 10, j * 4 - 80));
                    CreateRioter(new Vector3(i * 100 - 495, 10, j * 4 - 80));
                }
            }
        }

        public void RecruitNextRioter()
        {
            lock (rioters)
            {
                if (rioters.Count > 0)
                {
                    (rioters[rioters.Count - 1].GetComponent(typeof(RioterController)) as RioterController).MakePlayerMinion();
                }
                if (rioters.Count > 0)
                {
                    recruits.Add(rioters[rioters.Count - 1]);
                }

                if (rioters.Count > 0)
                {
                    rioters.RemoveAt(rioters.Count - 1);
                }
            }
        }

        List<GameEntity> rioters = new List<GameEntity>();
        public List<GameEntity> recruits = new List<GameEntity>();
        public void CreateRioter(Vector3 loc)
        {
            GameEntity entity = new GameEntity("derper");

            Entity box = new Box(loc, 2f, 4, 2f, 4);
            box.Material.KineticFriction = 0;
            box.Material.KineticFriction = 0;
            box.LocalInertiaTensorInverse = new Matrix3X3();
            box.CollisionInformation.CollisionRules.Group = (Game as Game1).guyCollisionGroup;
            entity.AddSharedData(typeof(Entity), box);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            Model guy = GetAnimatedModel("Models\\Player\\k_idle1");
            AnimationPlayer anims = new AnimationPlayer(guy.Tag as SkinningData);
            entity.AddSharedData(typeof(AnimationPlayer), anims);

            AnimatedModelComponent graphics = new AnimatedModelComponent(mainGame, entity, guy, 1, Vector3.Down * 2);
            entity.AddComponent(typeof(AnimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            RioterController controller = new RioterController(mainGame, entity);
            entity.AddComponent(typeof(RioterController), controller);
            genComponentManager.AddComponent(controller);

            rioters.Add(entity);
        }

        GameEntity level;
        public void CreateLevel()
        {
            Vector3 levelScale = new Vector3(5);
            GameEntity entity = new GameEntity("environment");

            Entity locationHolder = new Box(Vector3.Zero, 1, 1, 1);
            entity.AddSharedData(typeof(Entity), locationHolder);

            Model roomModel = GetUnanimatedModel("Models\\welch");
            Vector3[] vertices;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(roomModel, out vertices, out indices);
            StaticMesh roomMesh = new StaticMesh(vertices, indices, new AffineTransform(levelScale, Quaternion.Identity, Vector3.Zero));
            entity.AddSharedData(typeof(StaticMesh), roomMesh);

            StaticMeshComponent roomPhysics = new StaticMeshComponent(mainGame, entity);
            entity.AddComponent(typeof(StaticMeshComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, roomModel, levelScale, Vector3.Zero, 0, 0, 0);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            level = entity;

            Matrix transform = Matrix.CreateScale(levelScale) * Matrix.CreateRotationY(MathHelper.PiOver2);
            LevelTagData tag = roomModel.Tag as LevelTagData;
            if (tag != null)
            {
                List<Vector3> lightPoles = tag.lightPoleLocations;
                foreach (Vector3 vec in lightPoles)
                {
                    CreateLightPole(Vector3.Transform(vec, transform) + Vector3.Up * 15);
                }

                List<Vector3> cars = tag.cars;
                foreach (Vector3 vec in cars)
                {
                    CreateCar(Vector3.Transform(vec, transform) + Vector3.Up * 15);
                }

                List<Vector3> boxes1 = tag.mailbox1;
                foreach (Vector3 vec in boxes1)
                {
                    CreateMailbox1(Vector3.Transform(vec, transform) + Vector3.Up * 5);
                }

                List<Vector3> boxes2 = tag.mailbox2;
                foreach (Vector3 vec in boxes2)
                {
                    CreateMailbox2(Vector3.Transform(vec, transform) + Vector3.Up * 5);
                }

                List<Vector3> beer = tag.beer;
                foreach (Vector3 vec in beer)
                {
                    CreateBeer(Vector3.Transform(vec, transform) + Vector3.Up * 5);
                }

                CreateBeer(new Vector3(10000, 10000, 10000));
            }

            CreateSkybox();
        }

        private void CreateSkybox()
        {
            GameEntity entity = new GameEntity("");

            Entity locationHolder = new Box(Vector3.Zero, 0, 0, 0);
            entity.AddSharedData(typeof(Entity), locationHolder);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\skybox"), new Vector3(5), Vector3.Zero, 0, 0, 0);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);


        }

        List<GameEntity> lightPoles = new List<GameEntity>();
        public void CreateLightPole(Vector3 pos)
        {
            GameEntity entity = new GameEntity("prop");

            Entity data = new Box(pos, 4, 18, 4, 800);
            data.CollisionInformation.LocalPosition += Vector3.Down * 4;
            entity.AddSharedData(typeof(Entity), data);

            PhysicsComponent roomPhysics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\lamp"), new Vector3(4), Vector3.Down * 16.5f, 0, 0, 0);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            LightPoleController controller = new LightPoleController(mainGame, entity);
            entity.AddComponent(typeof(LightPoleController), controller);
            genComponentManager.AddComponent(controller);

            camera.AddLightPoleEntity(data);

            lightPoles.Add(entity);
        }

        public void CreateMailbox1(Vector3 pos)
        {
            GameEntity entity = new GameEntity("prop");

            Entity data = new Box(pos, 2, 3.75f, 2, 200);
            data.CollisionInformation.LocalPosition += Vector3.Down * 1;
            entity.AddSharedData(typeof(Entity), data);

            PhysicsComponent roomPhysics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\mailbox"), new Vector3(1), Vector3.Down * 2, 0, 0, 0);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            MailboxController controller = new MailboxController(mainGame, entity);
            entity.AddComponent(typeof(MailboxController), controller);
            genComponentManager.AddComponent(controller);

            lightPoles.Add(entity);
        }

        public void CreateMailbox2(Vector3 pos)
        {
            GameEntity entity = new GameEntity("prop");

            Entity data = new Box(pos, 2, 3.75f, 2, 200);
            data.CollisionInformation.LocalPosition += Vector3.Down * 1;
            entity.AddSharedData(typeof(Entity), data);

            PhysicsComponent roomPhysics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\mailbox2"), new Vector3(1), Vector3.Down * 2, 0, 0, 0);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            MailboxController controller = new MailboxController(mainGame, entity);
            entity.AddComponent(typeof(MailboxController), controller);
            genComponentManager.AddComponent(controller);

            lightPoles.Add(entity);
        }

        List<GameEntity> cars = new List<GameEntity>();
        public void CreateCar(Vector3 pos)
        {
            GameEntity entity = new GameEntity("prop");

            Entity data = new Box(pos, 8, 13, 11, 500);
            data.CollisionInformation.LocalPosition += Vector3.Down * 8;
            entity.AddSharedData(typeof(Entity), data);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\jeep"), new Vector3(2, 4, 2), Vector3.Down * 16.5f, 0, 0, 0);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            CarController controller = new CarController(mainGame, entity);
            entity.AddComponent(typeof(CarController), controller);
            genComponentManager.AddComponent(controller);

            cars.Add(entity);
        }

        public void CreatePunch(Vector3 pos, Vector3 dir, bool player)
        {
            GameEntity entity = new GameEntity("punch");

            Entity data = new Box(pos, .5f, .5f, .5f, 300);
            data.CollisionInformation.CollisionRules.Group = (Game as Game1).punchCollisionGroup;
            data.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;

            data.LinearVelocity = dir * 50;
            entity.AddSharedData(typeof(Entity), data);

            PhysicsComponent roomPhysics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            PunchController controller = new PunchController(mainGame, entity, player);
            entity.AddComponent(typeof(PunchController), controller);
            genComponentManager.AddComponent(controller);
        }

        public void CreateBeer(Vector3 loc)
        {
            GameEntity entity = new GameEntity("");

            Entity box = new Box(loc, 6, 2, 6);
            box.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            entity.AddSharedData(typeof(Entity), box);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\beer"), new Vector3(3), Vector3.Zero, 0, 0, 0);
            graphics.AddYawSpeed(.1f);
            graphics.AddEmitter(typeof(BeerGlowSystem), "glow", 8, 0, Vector3.Up * 8);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            BeerController controller = new BeerController(mainGame, entity);
            entity.AddComponent(typeof(BeerController), controller);
            genComponentManager.AddComponent(controller);


        }

        public void AttractMinions(GameEntity ent)
        {
            foreach (GameEntity r in recruits)
            {
                RioterController c = r.GetComponent(typeof(RioterController)) as RioterController;
                c.AttackObject(ent);
            }
        }
    }
}
