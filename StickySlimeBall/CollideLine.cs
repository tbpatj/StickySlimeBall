using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;

namespace StickySlimeBall
{
    class CollideLine
    {
        public float[] MoreData;
        public float x1 = 0;
        public float y1 = 0;
        public float x2 = 0;
        public float y2 = 0;
        public int index1Refrence = -1;
        public int index2Refrence = -1;
        public int index3Refrence = -1;

        public bool activatedSpecial = false;
        public float type = 0;
        //Moving
        public bool isMovingPlatform = false;
        public int platformIndex = 0;
        //Properties
        public bool canStick = true;
        public Vector normal = new Vector();
        public Line lineShape;
        public List<double> lastDot = new List<double>();
    }
}
