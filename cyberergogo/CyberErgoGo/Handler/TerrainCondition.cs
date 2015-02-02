using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    class TerrainCondition:Condition
    {
        public int TerrainWidth {
            get { return (int)GetParameterValue(ParameterIdentifier.Width); } 
            set { SetParameter(ParameterIdentifier.Width, value); }
        }

        public void SetTerrainWidth(int value)
        {
            TerrainWidth = value;
            base.ConditionHasChanged();
        }

        public int TerrainHeight
        {
            get { return (int)GetParameterValue(ParameterIdentifier.Height); }
            set { SetParameter(ParameterIdentifier.Height, value); }
        }

        public void SetTerrainHeight(int value)
        {
            TerrainHeight = value;
            base.ConditionHasChanged();
        }

        public float ColorHeightRate
        {
            get { return (float)GetParameterValue(ParameterIdentifier.ColorHeightRate); }
            set { SetParameter(ParameterIdentifier.ColorHeightRate, value); }
        }

        public float ZoomedColorHeightRate
        {
            get { return (float)GetParameterValue(ParameterIdentifier.ColorHeightRate) * (float)1 / (float)Zooming; }
        }

        public void SetColorHeightRate(int value)
        {
            ColorHeightRate = value;
            base.ConditionHasChanged();
        }

        public float Zooming
        {
            get { return (float)GetParameterValue(ParameterIdentifier.Zooming); }
            set { SetParameter(ParameterIdentifier.Zooming, value); }
        }

        public void SetZooming(int value)
        {
            Zooming = value;
            base.ConditionHasChanged();
        }

        public int MinHeight
        {
            get { return (int)GetParameterValue(ParameterIdentifier.MinHeight); }
            set { SetParameter(ParameterIdentifier.MinHeight, value); }
        }

        public void SetMinHeight(int value)
        {
            MinHeight = value;
            base.ConditionHasChanged();
        }

        public int MaxHeight
        {
            get { return (int)GetParameterValue(ParameterIdentifier.MaxHeight); }
            set { SetParameter(ParameterIdentifier.MaxHeight, value); }
        }

        public int AreaWidth
        {
            get { return (int)((int)(GetParameterValue(ParameterIdentifier.Width)) * (float)(GetParameterValue(ParameterIdentifier.Zooming))); }
        }

        public int AreaHeight
        {
            get { return (int)((int)(GetParameterValue(ParameterIdentifier.Height)) * (float)(GetParameterValue(ParameterIdentifier.Zooming))); }
        }

        public void SetMaxHeight(int value)
        {
            MaxHeight = value;
            base.ConditionHasChanged();
        }

        public Vector3[] Vertices
        {
            get { return (Vector3[])GetParameterValue(ParameterIdentifier.ShapeVertices); }
            set { SetParameter(ParameterIdentifier.ShapeVertices, value); }
        }

        public int[] Indices
        {
            get { return (int[])GetParameterValue(ParameterIdentifier.ShapeIndices); }
            set { SetParameter(ParameterIdentifier.ShapeIndices, value); }
        }

        public float[,] HeightValues
        {
            get { return (float[,])GetParameterValue(ParameterIdentifier.HeightValues); }
            set { SetParameter(ParameterIdentifier.HeightValues, value); }
        }

        public Texture2D Level0Texture
        {
            get { return (Texture2D)GetParameterValue(ParameterIdentifier.Level0Texture); }
            set { SetParameter(ParameterIdentifier.Level0Texture, value); }
        }

        public float Level0to1
        {
            get { return (float)GetParameterValue(ParameterIdentifier.Level0to1); }
            set { SetParameter(ParameterIdentifier.Level0to1, value); }
        }

        public Texture2D Level1Texture
        {
            get { return (Texture2D)GetParameterValue(ParameterIdentifier.Level1Texture); }
            set { SetParameter(ParameterIdentifier.Level1Texture, value); }
        }

        public float Level1to2
        {
            get { return (float)GetParameterValue(ParameterIdentifier.Level1to2); }
            set { SetParameter(ParameterIdentifier.Level1to2, value); }
        }

        public Texture2D Level2Texture
        {
            get { return (Texture2D)GetParameterValue(ParameterIdentifier.Level2Texture); }
            set { SetParameter(ParameterIdentifier.Level2Texture, value); }
        }

        public float Level2to3
        {
            get { return (float)GetParameterValue(ParameterIdentifier.Level2to3); }
            set { SetParameter(ParameterIdentifier.Level2to3, value); }
        }

        public Texture2D Level3Texture
        {
            get { return (Texture2D)GetParameterValue(ParameterIdentifier.Level3Texture); }
            set { SetParameter(ParameterIdentifier.Level3Texture, value); }
        }

        public Texture2D StreetTexture
        {
            get { return (Texture2D)GetParameterValue(ParameterIdentifier.StreetTexture); }
            set { SetParameter(ParameterIdentifier.StreetTexture, value); }
        }

        
        public TerrainCondition() : base(ConditionID.TerrainCondition) 
        {
            Parameters.Add(new Parameter(600, ParameterIdentifier.Width, ID));
            Parameters.Add(new Parameter(600, ParameterIdentifier.Height, ID));
            Parameters.Add(new Parameter(1f, ParameterIdentifier.Zooming, ID));
            Parameters.Add(new Parameter(1f, ParameterIdentifier.ColorHeightRate, ID));
            Parameters.Add(new Parameter(0, ParameterIdentifier.MinHeight, ID));
            Parameters.Add(new Parameter(0, ParameterIdentifier.MaxHeight, ID));
            Parameters.Add(new Parameter(new float[0,0], ParameterIdentifier.HeightValues, ID));

            //textures and textureWeights
            Parameters.Add(new Parameter(0.3f, ParameterIdentifier.Level0to1, ID));
            Parameters.Add(new Parameter(0.6f, ParameterIdentifier.Level1to2, ID));
            Parameters.Add(new Parameter(0.9f, ParameterIdentifier.Level2to3, ID));
            Texture2D level0texture = null;
            Util.GetInstance().LoadFile(ref level0texture, "Terrain", "sand");
            Texture2D level1texture = null;
            Util.GetInstance().LoadFile(ref level1texture, "Terrain", "gras");
            Texture2D level2texture =  null;
            Util.GetInstance().LoadFile(ref level2texture, "Terrain", "earth");
            Texture2D level3texture = null;
            Util.GetInstance().LoadFile(ref level3texture, "Terrain", "rock");
            Texture2D streetTexture = null;
            Util.GetInstance().LoadFile(ref streetTexture, "Terrain", "street");

            Parameters.Add(new Parameter(level0texture, ParameterIdentifier.Level0Texture, ID));
            Parameters.Add(new Parameter(level1texture, ParameterIdentifier.Level1Texture, ID));
            Parameters.Add(new Parameter(level2texture, ParameterIdentifier.Level2Texture, ID));
            Parameters.Add(new Parameter(level3texture, ParameterIdentifier.Level3Texture, ID));
            Parameters.Add(new Parameter(streetTexture, ParameterIdentifier.StreetTexture, ID));
        }
    }
}
