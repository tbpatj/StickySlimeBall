using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StickySlimeBall
{
    class Player
    {
        public double xPos = 5100;
        public double yPos = -4100;
        public double yVel = 0;
        public double xVel = 0;
        public double lastSafeXPos = 5100;
        public double lastSafeYPos = -4100;
        public double ballSize = 25;
        public int stickyBoost = 200;
        public int maxBoost = 300;
        public int died = 0;
        public double addedXVel = 0;
        public double addedYVel = 0;
        //player collision/surface properties
        public bool touching = false;
        public int touchingIndex = -1;
        public int lastSafeTouchingIndex = -1;
        public Vector touchingNormal = new Vector();
        public Vector lastSafeTouchingNormal = new Vector();
        public Vector lastSafeGravity = new Vector(0, -0.1);
        public int maxTouchingCountDown = 15;
        public int movingPlatformTouch = -1;
        public int stickingOn = 1;
        public int liftedUpSinceTouched = 0;
        public int lastTouchingTime = 0;

        //camera qualities
        public double lastSafeCamRotate = 0;
    }
}
