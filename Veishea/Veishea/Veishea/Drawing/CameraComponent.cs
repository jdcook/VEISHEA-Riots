using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.ResourceManagement;

namespace Veishea
{
    public class CameraComponent : GameComponent
    {
        #region Camera Fields
        private float fov = MathHelper.PiOver4;
        private float nearPlane = .1f;
        private float farPlane = 2000;

        public Entity physicalData { get; private set; }

        private Matrix inverseViewProj = Matrix.Identity;
        public Matrix InverseViewProj { get { return inverseViewProj; } }

        private Matrix proj;
        public Matrix Projection { get { return proj; } }

        private Matrix view;
        public Matrix View { get { return view; } }

        private Vector3 position;
        public Vector3 Position { get { return position; } }

        private Vector3 target;
        public Vector3 Target { get { return target; } }

        private Matrix rot;
        private Vector3 rotatedTarget;
        private Vector3 rotatedUpVector;

        private Vector3 headOffset = new Vector3(0, 3, 0);

        float distanceFromTarget = 10;

        float yaw = MathHelper.PiOver4;
        float pitch = -MathHelper.PiOver4;
        public float zoom = 1f;
        private float maxZoom = 4;
        private float minZoom = .2f;
        #endregion

        List<Entity> lightPoleEntities = new List<Entity>();
        public void AssignEntity(Entity followMe)
        {
            this.physicalData = followMe;
        }

        private Vector3 inactiveLightPos = new Vector3(-10000, 0, 0);
        public Vector3[] lightPositions = new Vector3[10];
        public static readonly int NUM_LIGHTS = 10;
        public CameraComponent(Game1 game)
            : base(game)
        {
            this.UpdateOrder = 2;
            for (int i = 0; i < NUM_LIGHTS; ++i)
            {
                lightPositions[i] = inactiveLightPos;
            }

        }

        public void AddLightPoleEntity(Entity e)
        {
            lightPoleEntities.Add(e);
        }

        public override void Initialize()
        {

            proj = Matrix.CreatePerspectiveFieldOfView(fov, Game.GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);

            rot = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
            rotatedTarget = Vector3.Transform(new Vector3(0, 0, -1), rot);
            rotatedUpVector = Vector3.Transform(new Vector3(0, 1, 0), rot);

            int sheight = Game.GraphicsDevice.Viewport.Height;
            int swidth = Game.GraphicsDevice.Viewport.Width;
        }

        MouseState curMouse = Mouse.GetState();
        MouseState prevMouse = Mouse.GetState();

        KeyboardState curKeys = Keyboard.GetState();
        KeyboardState prevKeys = Keyboard.GetState();
        public override void Update(GameTime gameTime)
        {
            curMouse = Mouse.GetState();
            curKeys = Keyboard.GetState();

            if (Game.IsActive)
            {
                if (curMouse.ScrollWheelValue < prevMouse.ScrollWheelValue)
                {
                    zoom *= 1.2f;
                }
                else if (curMouse.ScrollWheelValue > prevMouse.ScrollWheelValue)
                {
                    zoom /= 1.2f;
                }
                if (zoom < minZoom)
                {
                    zoom = minZoom;
                }
                if (zoom > maxZoom)
                {
                    zoom = maxZoom;
                }


                int w = Game.GraphicsDevice.Viewport.Width / 2;
                int h = Game.GraphicsDevice.Viewport.Height / 2;

                float dx = curMouse.X - w;
                float dy = curMouse.Y - h;
                Mouse.SetPosition(w, h);

                yaw -= dx * .005f;
                pitch -= dy * .005f;
                if (pitch > 0)
                {
                    pitch = 0;
                }
                else if (pitch < -MathHelper.PiOver2)
                {
                    pitch = -MathHelper.PiOver2;
                }

                rot = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
                rotatedTarget = Vector3.Transform(new Vector3(0, 0, -1), rot);
                rotatedUpVector = Vector3.Transform(new Vector3(0, 1, 0), rot);

                distanceFromTarget = 20 * zoom;
                target = physicalData.Position + headOffset;
                position = target + rot.Backward * distanceFromTarget;
                view = Matrix.CreateLookAt(position, target, rotatedUpVector);

                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
            }

            int i = 0;
            foreach (Entity e in lightPoleEntities)
            {
                if (i < lightPositions.Length)
                {
                    lightPositions[i++] = e.Position + e.OrientationMatrix.Up * 4;
                }
            }
            while (i < lightPositions.Length)
            {
                lightPositions[i++] = inactiveLightPos;
            }

            prevMouse = curMouse;
            prevKeys = curKeys;
        }
    }
}
