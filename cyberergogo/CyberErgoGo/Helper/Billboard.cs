using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    /// <summary>
    /// This class describes an billboard in worldposition. 
    /// </summary>
    class Billboard
    {
        protected Vector3 WorldPosition;
        protected Quaternion Rotation = Quaternion.Identity;
        protected float Height;
        protected Texture2D Texture;
        protected float Width;

        protected bool Flip = false;
        protected bool IsAnimated = false;

        protected VertexBuffer VBuffer;
        protected IndexBuffer IBuffer;
        protected String TextureName;
        protected String TextureFolder;
        public bool ToDraw = true;

        protected Vector3 Translation = Vector3.Zero;

        public Billboard(float height, Vector3 pos, String textureFolder, String textureName)
        {
            TextureFolder = textureFolder;
            TextureName = textureName;

            WorldPosition = pos + new Vector3(0,height/2,0); ;
            Height = height;
            
            
            if(Util.GetInstance().GetRandomNumber(2) == 1)
            Flip = true;

            SetUpBuffer();
        }

        public Billboard(float height, Vector3 pos, Texture2D texture)
        {
            TextureFolder = "";
            TextureName = "";

            Texture = texture;

            WorldPosition = pos + new Vector3(0, height, 0); ;
            Height = height;


            if (Util.GetInstance().GetRandomNumber(2) == 1)
                Flip = true;

            SetUpBuffer();
        }

        protected Billboard()
        {
        }


        public virtual void LoadContent()
        {
            if(TextureFolder != "")
                Util.GetInstance().LoadFile(ref Texture, TextureFolder, TextureName);
            float ratio = (float)Texture.Width / (float)Texture.Height;
            Width = Height * ratio;
        }

        protected void SetUpBuffer()
        {

            Vector2 lb = new Vector2(0, 0);
            Vector2 rb = new Vector2(1, 0);
            Vector2 lt = new Vector2(0, 1);
            Vector2 rt = new Vector2(1, 1);

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(WorldPosition, lb);
            vertices[1] = new VertexPositionTexture(WorldPosition, lt);
            vertices[2] = new VertexPositionTexture(WorldPosition, rt);
            vertices[3] = new VertexPositionTexture(WorldPosition, rb);

            VBuffer = new VertexBuffer(Util.GetInstance().Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            VBuffer.SetData(vertices);

            int[] indeces = new int[6];
            indeces[0] = 0;
            indeces[1] = 2;
            indeces[2] = 1;
            indeces[3] = 0;
            indeces[4] = 3;
            indeces[5] = 2;

            IBuffer = new IndexBuffer(Util.GetInstance().Device, typeof(int), indeces.Length, BufferUsage.WriteOnly);
            IBuffer.SetData(indeces);
        }

        public VertexBuffer GetVertexBuffer()
        {
            return VBuffer;
        }

        public IndexBuffer GetIndexBuffer()
        {
            return IBuffer;
        }

        public Matrix GetRotationMatrix()
        {
            return Matrix.CreateFromQuaternion(Rotation);
        }

        public Vector3 GetWorldPosition()
        {
            return WorldPosition;
        }

        public void ChangeTexture(Texture2D newTex)
        {
            Texture = newTex;
        }

        public void ChangeWorldPosition(Vector3 newPosition)
        {
            Translation = newPosition - WorldPosition;
        }

        public void Translate(Vector3 translation)
        {
            Translation += translation;
        }

        public void ChangeRotation(Quaternion newRotation)
        {
            Rotation = newRotation;
        }

        public float GetWidth()
        {
            return Width;
        }

        public float GetHeight()
        {
            return Height;
        }

        public Texture2D GetTexture()
        {
            return Texture;
        }

        public bool GetFlip()
        {
            return Flip;
        }

        public bool GetIsAnimated()
        {
            return IsAnimated;
        }

        public Vector2 GetRelativePosition(int width, int height)
        {
            if (WorldPosition.X > 0 && WorldPosition.Z > 0)
                return new Vector2((float)(WorldPosition.X / (float)width), (float)(WorldPosition.Z / (float)height));
            else
                return new Vector2(0, 0);
        }

        public Vector3 GetTranslation()
        {
            return Translation;
        }
    }
}
