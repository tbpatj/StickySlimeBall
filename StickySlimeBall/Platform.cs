using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace StickySlimeBall
{
    class Platform
    {
        public float movePoint1X = 0;
        public float movePoint1Y = 0;
        public float movePoint2X = 0;
        public float movePoint2Y = 0;
        public int wait = 0;
        public int waitTime = 10;
        public float lastDis = 10000;

        public Vector position = new Vector(0, 0);
        public Vector platformSize = new Vector(0, 0);
        public Vector vel = new Vector(0, 0);
        public bool movingTo1 = false;
        public float moveSpeed = 0;
        public int index1 = 0;
        public int index2 = 0;
        public int index3 = 0;
        public int index4 = 0;
    }
}
