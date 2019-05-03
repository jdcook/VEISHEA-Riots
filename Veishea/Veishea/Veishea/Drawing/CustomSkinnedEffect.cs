using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Veishea
{
    /// <summary>
    /// Built-in effect for rendering skinned character models.
    /// </summary>
    public class CustomSkinnedEffect : Effect, IEffectMatrices
    {
        public const int MaxBones = 72;

        #region Effect Parameters

        EffectParameter textureParam;
        EffectParameter diffuseColorParam;
        EffectParameter emissiveColorParam;
        EffectParameter specularColorParam;
        EffectParameter specularPowerParam;
        EffectParameter eyePositionParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;
        EffectParameter worldParam;
        EffectParameter worldInverseTransposeParam;
        EffectParameter worldViewProjParam;
        EffectParameter bonesParam;
        EffectParameter shaderIndexParam;

        EffectParameter lightPositionsParam;
        EffectParameter lightColorsParam;

        #endregion

        #region Fields

        bool preferPerPixelLighting;
        bool fogEnabled;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;
        Vector3 emissiveColor = Vector3.Zero;
        Vector3 ambientLightColor = Vector3.Zero;

        float alpha = 1;

        float fogStart = 0;
        float fogEnd = 1;

        int weightsPerVertex = 4;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        #endregion

        #region Public Properties

        public Vector3[] LightPositions
        {
            set
            {
                this.lightPositionsParam.SetValue(value);
            }
        }
        public Vector3[] LightColors
        {
            set
            {
                this.lightColorsParam.SetValue(value);
            }
        }

        

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }

            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }

            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }

            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }


        /// <summary>
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }

            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material emissive color (range 0 to 1).
        /// </summary>
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }

            set
            {
                emissiveColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material specular color (range 0 to 1).
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColorParam.GetValueVector3(); }
            set { specularColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the material specular power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPowerParam.GetValueSingle(); }
            set { specularPowerParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the material alpha.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }

            set
            {
                alpha = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the per-pixel lighting prefer flag.
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get { return preferPerPixelLighting; }

            set
            {
                if (preferPerPixelLighting != value)
                {
                    preferPerPixelLighting = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }


        /// <summary>
        /// Gets or sets the ambient light color (range 0 to 1).
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }

            set
            {
                ambientLightColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the current texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return textureParam.GetValueTexture2D(); }
            set { textureParam.SetValue(value); }
        }



        /// <summary>
        /// Sets an array of skinning bone transform matrices.
        /// </summary>
        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            if ((boneTransforms == null) || (boneTransforms.Length == 0))
                throw new ArgumentNullException("boneTransforms");

            if (boneTransforms.Length > MaxBones)
                throw new ArgumentException("Too many bones");

            bonesParam.SetValue(boneTransforms);
        }


        /// <summary>
        /// Gets a copy of the current skinning bone transform matrices.
        /// </summary>
        public Matrix[] GetBoneTransforms(int count)
        {
            if (count <= 0 || count > MaxBones)
                throw new ArgumentOutOfRangeException("count");

            Matrix[] bones = bonesParam.GetValueMatrixArray(count);

            // Convert matrices from 43 to 44 format.
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].M44 = 1;
            }

            return bones;
        }



        #endregion

        #region Methods


        /// <summary>
        /// Creates a new SkinnedEffect with default parameter settings.
        /// </summary>
        public CustomSkinnedEffect(Effect effect)
            : base(effect)
        {
            CacheEffectParameters(null);

            SpecularColor = Vector3.One;
            SpecularPower = 16;

            Matrix[] identityBones = new Matrix[MaxBones];

            for (int i = 0; i < MaxBones; i++)
            {
                identityBones[i] = Matrix.Identity;
            }

            SetBoneTransforms(identityBones);
        }


        /// <summary>
        /// Creates a new SkinnedEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected CustomSkinnedEffect(SkinnedEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);

            preferPerPixelLighting = cloneSource.PreferPerPixelLighting;
            fogEnabled = cloneSource.FogEnabled;

            world = cloneSource.World;
            view = cloneSource.View;
            projection = cloneSource.Projection;

            diffuseColor = cloneSource.DiffuseColor;
            emissiveColor = cloneSource.EmissiveColor;
            ambientLightColor = cloneSource.AmbientLightColor;

            alpha = cloneSource.Alpha;

            fogStart = cloneSource.FogStart;
            fogEnd = cloneSource.FogEnd;

            weightsPerVertex = cloneSource.WeightsPerVertex;
        }

        public void CopyFromSkinnedEffect(SkinnedEffect cloneSource)
        {
            CacheEffectParameters(cloneSource);

            preferPerPixelLighting = cloneSource.PreferPerPixelLighting;
            fogEnabled = cloneSource.FogEnabled;

            world = cloneSource.World;
            view = cloneSource.View;
            projection = cloneSource.Projection;

            diffuseColor = cloneSource.DiffuseColor;
            emissiveColor = cloneSource.EmissiveColor;
            ambientLightColor = cloneSource.AmbientLightColor;

            alpha = cloneSource.Alpha;

            fogStart = cloneSource.FogStart;
            fogEnd = cloneSource.FogEnd;

            weightsPerVertex = cloneSource.WeightsPerVertex;

            Texture = cloneSource.Texture;
            SpecularColor = cloneSource.SpecularColor;
            SpecularPower = cloneSource.SpecularPower;

            eyePositionParam.SetValue(cloneSource.Parameters["EyePosition"].GetValueVector3());
            fogVectorParam.SetValue(cloneSource.Parameters["FogVector"].GetValueVector4());
            worldInverseTransposeParam.SetValue(cloneSource.Parameters["WorldInverseTranspose"].GetValueMatrix());
            bonesParam.SetValue(cloneSource.Parameters["Bones"].GetValueMatrixArray(MaxBones));
        }

        /// <summary>
        /// Creates a clone of the current SkinnedEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new CustomSkinnedEffect(this);
        }



        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters(SkinnedEffect cloneSource)
        {
            textureParam = Parameters["Texture"];
            diffuseColorParam = Parameters["DiffuseColor"];
            emissiveColorParam = Parameters["EmissiveColor"];
            specularColorParam = Parameters["SpecularColor"];
            specularPowerParam = Parameters["SpecularPower"];
            eyePositionParam = Parameters["EyePosition"];
            fogColorParam = Parameters["FogColor"];
            fogVectorParam = Parameters["FogVector"];
            worldParam = Parameters["World"];
            worldInverseTransposeParam = Parameters["WorldInverseTranspose"];
            worldViewProjParam = Parameters["WorldViewProj"];
            bonesParam = Parameters["Bones"];
            shaderIndexParam = Parameters["ShaderIndex"];
            lightPositionsParam = Parameters["lightPositions"];
            lightColorsParam = Parameters["lightColors"];

        }


        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected override void OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

            // Recompute the world inverse transpose and eye position?
            dirtyFlags = EffectHelpers.SetLightingMatrices(dirtyFlags, ref world, ref view, worldParam, worldInverseTransposeParam, eyePositionParam);
        }


        #endregion
    }
}
