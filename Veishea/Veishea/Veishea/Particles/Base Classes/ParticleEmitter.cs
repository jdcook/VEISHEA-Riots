using System;
using Microsoft.Xna.Framework;

namespace Veishea
{
    public class ParticleEmitter
    {
        #region Fields
        CameraComponent camera;
        public int BoneIndex { get; private set; }

        ParticleSystem particleSystem;
        float timeBetweenParticles;
        Vector3 previousPosition;
        float timeLeftOver;
        Random rand;
        int maxHorizontalOffset;
        int maxVerticalOffset;
        Vector3 offset;

        public bool Timed { get; private set; }
        public bool Dead{get; private set;}
        private double timeLeft = 0;
        #endregion

        public void SetHorizontalOffset(int offset)
        {
            this.maxHorizontalOffset = offset;
        }

        public void SetVerticalOffset(int offset)
        {
            this.maxVerticalOffset = offset;
        }

        /// <summary>
        /// Constructs a new particle emitter object.
        /// </summary>
        public ParticleEmitter(CameraComponent camera, ParticleSystem particleSystem,
                               float particlesPerSecond, Vector3 initialPosition, Vector3 offset)
        {
            this.camera = camera;
            this.particleSystem = particleSystem;

            timeBetweenParticles = 1.0f / particlesPerSecond;
            
            previousPosition = initialPosition;

            rand = new Random();
            this.offset = offset;

            this.BoneIndex = -1;

            Timed = false;
            Dead = false;
        }
        public ParticleEmitter(CameraComponent camera, ParticleSystem particleSystem,
                       float particlesPerSecond, Vector3 initialPosition, Vector3 offset, int attachIndex)
        {
            this.camera = camera;
            this.particleSystem = particleSystem;

            timeBetweenParticles = 1.0f / particlesPerSecond;

            previousPosition = initialPosition;

            rand = new Random();
            this.offset = offset;

            this.BoneIndex = attachIndex;

            Timed = false;
            Dead = false;
        }

        float sizePercent = 1;
        float sizeIncrement = 0;
        float sizeIncrementIncrement = 0;
        public void IncreaseSizePerSecond(float sizeIncrement)
        {
            this.sizeIncrement = sizeIncrement;
        }

        /// <summary>
        /// increases the size by sizeIncrement per second, and increases that increment by sizeIncrementIncrement per second.
        /// 
        /// Increment-ception!
        /// </summary>
        public void IncreaseSizePerSecondExponential(float sizeIncrement, float sizeIncrementIncrement)
        {
            this.sizeIncrement = sizeIncrement;
            this.sizeIncrementIncrement = sizeIncrementIncrement;
        }


        Vector3 extraVel = Vector3.Zero;
        public void SetVelocity(Vector3 vel)
        {
            extraVel = vel;
        }

        float alongUp = 0;
        public void SetAlongUpAmount(float amount)
        {
            this.alongUp = amount;
        }

        Vector3 up = Vector3.Up;
        public void SetUpTranslationVector(Vector3 up)
        {
            this.up = up;
        }
        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles
        /// in the appropriate positions.
        /// </summary>
        public void Update(GameTime gameTime, Vector3 newPosition, Matrix rotation)
        {
            if (Timed)
            {
                timeLeft -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (timeLeft <= 0)
                {
                    Dead = true;
                }
            }


            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (sizeIncrement != 0)
            {
                if (sizeIncrementIncrement != 0)
                {
                    sizeIncrement += sizeIncrementIncrement;
                }

                sizePercent += sizeIncrement * elapsedTime;
                if (sizePercent < 0)
                {
                    sizePercent = 0;
                }
            }

            if (elapsedTime > 0)
            {
                // Work out how fast we are moving.
                Vector3 velocity = (newPosition - previousPosition) / elapsedTime + extraVel;

                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + elapsedTime;
                
                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                while (timeToSpend > timeBetweenParticles)
                {
                    currentTime += timeBetweenParticles;
                    timeToSpend -= timeBetweenParticles;

                    float mu = currentTime / elapsedTime;

                    Vector3 position = Vector3.Lerp(previousPosition, newPosition, mu);

                    // Create the particle.
                    Vector3 finalpos = Vector3.Transform(offset, rotation);
                    finalpos += position + up * alongUp;

                    particleSystem.AddParticle(finalpos + new Vector3(rand.Next(maxHorizontalOffset * 2) - maxHorizontalOffset, rand.Next(maxVerticalOffset * 2) - maxVerticalOffset, rand.Next(maxHorizontalOffset * 2) - maxHorizontalOffset), velocity, sizePercent);
                    
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }

            previousPosition = newPosition;
        }

        protected bool InsideCameraBox(BoundingBox cameraBox, Vector3 position)
        {
            Vector3 pos = position;
            return !(pos.X < cameraBox.Min.X
                || pos.X > cameraBox.Max.X
                || pos.Z < cameraBox.Min.Z
                || pos.Z > cameraBox.Max.Z);
        }


        public void SetDeathTimer(double timerLength)
        {
            if (timerLength > timeLeft)
            {
                timeLeft = timerLength;
                Timed = true;
            }
        }
    }
}
