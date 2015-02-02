using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    class BillboardTree:Billboard
    {
        
        const int NumberOfPossibleTrees = 3;
        

        public BillboardTree(int height, Vector3 pos)
            : base(GetRandomHeight(height), pos, "Environment", GetRandomTreeName())
        {            

        }

        private static int GetRandomHeight(int height)
        {
            int halfheight = height / 2;
            return height + (Util.GetInstance().GetRandomNumber(halfheight)) - halfheight / 2;
        }

        

        private static String GetRandomTreeName()
        {
            int treeNr = Util.GetInstance().GetRandomNumber(NumberOfPossibleTrees);
            return  "Tree" + treeNr;
        }

       
    }
}
