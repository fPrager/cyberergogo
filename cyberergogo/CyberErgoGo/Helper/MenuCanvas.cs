using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class MenuCanvas
    {
        private Texture2D[] Textures;
        private int CurrentTextureIndex = 0;
        private VertexBuffer VBuffer;
        private IndexBuffer IBuffer;

        bool InWorldSpace = true;

        public Vector3 Position;

        private float Width;
        private float Height;

        public Vector3 Color = new Vector3(1,1,1);
        public bool AsMask = false;

        public float Opacity;

        public float PositionX
        {
            get { return Position.X; }
        }

        public float PositionY
        {
            get { return Position.Y; }
        }

        public float PositionZ
        {
            get { return Position.Z; }
        }

        public Texture CurrentTexture
        { 
            get { return Textures[CurrentTextureIndex];}
        }

        public MenuCanvas(Vector3 position, float width, float height, Texture2D[] textures, float opacity)
        {
            Width = width;
            Height = height;
            Textures = textures;
            Position = position;
            Opacity = opacity;
            SetUpBuffer();
        }

        public MenuCanvas(Vector3 position, float width, float height, Texture2D[] textures, float opacity, Vector3 color):this(position,  width,  height,textures, opacity)
        {
            Color = color;
            AsMask = true;
        }

        public MenuCanvas(Vector2 position, float zOffset, float width, float height, Texture2D[] textures, float opacity, Vector3 color):this( position,  zOffset,  width,  height,  textures, opacity)
        {
            Color = color;
            AsMask = true;
        }

        public MenuCanvas(Vector2 position, float zOffset, float width, float height, Texture2D[] textures, float opacity)
        {
            Width = width;
            Height = height;
            Textures = textures;
            Position = new Vector3(position.X, position.Y, zOffset);
            InWorldSpace = false;
            Opacity = opacity;
            SetUpBuffer();
        }

        public void NextTexture()
        {
            CurrentTextureIndex = (CurrentTextureIndex+1) % Textures.Length;
        }

        public void ChangePosition(Vector2 position)
        {
            Position.X = position.X;
            Position.Y = position.Y;
            InWorldSpace = false;
        }

        public void ChangePosition(Vector3 position)
        {
            Position.X = position.X;
            Position.Y = position.Y;
            Position.Z = position.Z;
            InWorldSpace = true;
        }

        public void ChangeZOffset(float zOffset)
        {
            Position.Z = zOffset;
            InWorldSpace = false;
        }

        private void SetUpBuffer()
        {
            Vector2 lb = new Vector2(0, 0);
            Vector2 rb = new Vector2(1, 0);
            Vector2 lt = new Vector2(0, 1);
            Vector2 rt = new Vector2(1, 1);

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            float xOffset = Width / 2f;
            float yOffset = Height / 2f;

            vertices[0] = new VertexPositionTexture(new Vector3(-xOffset, -yOffset, 0), lb);
            vertices[1] = new VertexPositionTexture(new Vector3(-xOffset, yOffset, 0), lt);
            vertices[2] = new VertexPositionTexture(new Vector3(xOffset, yOffset, 0), rt);
            vertices[3] = new VertexPositionTexture(new Vector3(xOffset,- yOffset, 0), rb);

            VBuffer = new VertexBuffer(Util.GetInstance().Device, VertexPositionTexture.VertexDeclaration,4, BufferUsage.WriteOnly);
            VBuffer.SetData(vertices);

            int[] indeces = new int[6];
            indeces[0] = 0;
            indeces[1] = 1;
            indeces[2] = 2;
            indeces[3] = 2;
            indeces[4] = 3;
            indeces[5] = 0;

            IBuffer = new IndexBuffer(Util.GetInstance().Device, typeof(int), indeces.Length, BufferUsage.WriteOnly);
            IBuffer.SetData(indeces);
        }

        public VertexBuffer GetVBuffer()
        {
            return VBuffer;
        }

        public IndexBuffer GetIBuffer()
        {
            return IBuffer;
        }
    }
}
