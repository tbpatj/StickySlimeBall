using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


//Old code from 2018


namespace StickySlimeBall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    
    public partial class MainWindow : Window
    {
        //objects and lists that store all the items
        List<CollideLine> collides = new List<CollideLine>();
        List<BallCollide> bCollides = new List<BallCollide>();
        List<Particle> particles = new List<Particle>();
        List<Platform> movingPlatforms = new List<Platform>();
        List<InfoText> Information = new List<InfoText>();
        DispatcherTimer timer;

        //key input
        bool keyUp;
        bool keyDown;
        bool keyRight;
        bool keyLeft;

        //world properties
        Vector gravity = new Vector(0,-0.1);
        bool friction = true;
        bool canStick = false;

        Player player = new Player();

        //player properties
        /**
        double xPos = 5100;
        double yPos = -4100;
        double yVel = 0;
        double xVel = 0;
        double lastSafeXPos = 5100;
        double lastSafeYPos = -4100;
        double ballSize = 25;
        int stickyBoost = 200;
        int maxBoost = 300;
        int died = 0;
        double addedXVel = 0;
        double addedYVel = 0;
        //player collision/surface properties
        bool touching = false;
        int touchingIndex = -1;
        int lastSafeTouchingIndex = -1;
        Vector touchingNormal = new Vector();
        Vector lastSafeTouchingNormal = new Vector();
        Vector lastSafeGravity = new Vector(0, -0.1);
        int maxTouchingCountDown = 15;
        int movingPlatformTouch = -1;
        int stickingOn = 1;
        int liftedUpSinceTouched = 0;
        int lastTouchingTime = 0;
        */

        //camera properties
        double cameraX = 0;
        double cameraY = 0;
        double zoom = 0.6;
        double camRotate = 0;
        float rotateTo = 0;
        bool lockCam = true;
        float CamDis = 0;
        

        // gameplay/gameflow
        int finalSwitch = 0;
        bool paused = false;
        int Score = 0;
        int pauseReason = 0;
        int addedSecond = 0;
        int addedMinute = 0;
        int lastSecond = 0;
        int lastMinute = 0;
        int attatchedCoolDown = 0;
        DateTime startTime = new DateTime();
        int level = 1;
        int connectSize = 5;
        Line stickyBoostShown = new Line();
        Random r = new Random();
        public MainWindow()
        {
            //set up the sticky boost bar
            InitializeComponent();
            stickyBoostShown.X1 = 100;
            stickyBoostShown.Y1 = MyCanvas.ActualHeight + 100;
            stickyBoostShown.X2 = MyCanvas.ActualWidth - 100;
            stickyBoostShown.Y2 = MyCanvas.ActualHeight + 100;
            stickyBoostShown.Stroke = Brushes.DarkGreen;
            stickyBoostShown.StrokeThickness = 2;
            MyCanvas.Children.Add(stickyBoostShown);
           
            //load up the level
            level = 1;
            LoadLevel(level);
                
            //calculate all the surface normals
            GoThroughLines(false, true,true);
                
            //set up the timer for gameplay and score
            DateTime now = DateTime.Now;
            addedSecond = now.Second;
            addedMinute = now.Minute;
            startTime = now;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void CreateBlock(int x, int y, int xSize, int ySize, bool sticks1, bool sticks2, bool sticks3, bool sticks4)
        {
            CreateLine(x - xSize / 2, y - ySize / 2, x + xSize / 2, y - ySize / 2, true, sticks1);
            CreateLine(x + xSize / 2, y - ySize / 2, x + xSize / 2, y + ySize / 2, true, sticks2);
            CreateLine(x + xSize / 2, y + ySize / 2, x - xSize / 2, y + ySize / 2, true, sticks3);
            CreateLine(x - xSize / 2, y + ySize / 2, x - xSize / 2, y - ySize / 2, true, sticks4);
        }
        private void LoadLevel(int levelNum)
        {
            //remove all previous children and objects inside the world
            for (int i = 0; i < collides.Count(); i++)
            {
                CollideLine c = collides[i];
                MyCanvas.Children.Remove(c.lineShape);
            }
            for (int i = 0; i < bCollides.Count(); i++)
            {
                BallCollide c = bCollides[i];
                MyCanvas.Children.Remove(c.ball);
            }
            for (int i = 0; i < particles.Count(); i++)
            {
                Particle c = particles[i];
                MyCanvas.Children.Remove(c.shape);
            }
            for (int i = 0; i < Information.Count(); i++)
            {
                InfoText c = Information[i];
                MyCanvas.Children.Remove(c.textInfo);
            }
            collides.Clear();
            bCollides.Clear();
            particles.Clear();
            movingPlatforms.Clear();
            Information.Clear();
            collides = new List<CollideLine>();
            bCollides = new List<BallCollide>();
            particles = new List<Particle>();
            movingPlatforms = new List<Platform>();
            Information = new List<InfoText>();

            //reset the timer
            DateTime now = DateTime.Now;
            addedSecond = now.Second;
            addedMinute = now.Minute;
            startTime = now;
            //set the player to not touching anything
            player.touchingIndex = -1;
            //lock the camera to the player
            lockCam = true;
            if (levelNum == 1)
            {
                //reset the player position and speed, and boost
                player.xPos = 0;
                player.yPos = 0;
                player.xVel = 0;
                player.yVel = 0;
                player.maxBoost = 300;
                player.stickyBoost = player.maxBoost;

                //reset the camera position and properties
                cameraX = 0;
                cameraY = 0;
                camRotate = 0;
                rotateTo = 0;

                //reset the world properties
                gravity.X = 0;
                gravity.Y = -0.1;
                //load up all the level resources, such as the lines
                CreateLevel1();
                //set up the normals
                GoThroughLines(false, true, true);

                //set the render for the player according to the location and elements
                Canvas.SetLeft(Ball, (player.xPos * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2);
                Canvas.SetTop(Ball, (player.yPos * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2);
            }
            else if(levelNum == 2)
            {
                //set player location and speed and boost
                player.xPos = -1180;
                player.yPos = 2750;
                player.xVel = 0;
                player.yVel = 0;
                player.maxBoost = 300;
                player.stickyBoost = player.maxBoost;

                //set the camera location
                cameraX = 1100;
                cameraY = -2740;
                camRotate = 0;

                //set up the geometry and world collisions
                SetUpLines();
                //set up the normals
                GoThroughLines(false, true, true);

                //set the render for the player according to the location and elements
                Canvas.SetLeft(Ball, (player.xPos * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2);
                Canvas.SetTop(Ball, (player.yPos * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2);
            }
            else if(levelNum == 3)
            {
                pauseReason = 5;
            }
        }
        private void createlevel3()
        {
            player.xPos = 0;
            player.yPos = 0;
            level = 3;
            CreateLine(-1000, 200, 1000, 200, true, true);
            CreateLine(300, 200, 300, 0, true, true);
            int index = CreateElectrical(4000, -4990, collides.Count + 1);
            //int index2 = CreateElectrical(4000, -4990, collides.Count + 2);
            MovingLine(-400, 0, 0, 200, -1000, 0, 200, index, false,true,false);
            //MovingLine(0,200, 400, 0, -300, 0, 50, index2, false, true, false);
            //MovingLine(-400, 50, -200, 100, -1000, 0, 500, index, false, true, false);
            //CreateMovingBlock(0, 210, 300, 80, 75, -1000, 0, false,500, index, true);
            //CreateMovingBlock(500, 210, 300, 80, 75, -1000, 0, false, 500, index, true);
        }
        
        private void CreateLevel1()
        {

            //create texts
            CreateInfo("To move left use A/Left.", 0, 125, 0,0, 200, 40, 14, true, 300);
            CreateInfo("To move right use D/right.", -325, 125, -250, 0, 200, 40, 14, true, 300);
            CreateInfo("You can stick to green walls. \n If trying to stick, while touching a surface \n try using the up key to switch more easily.", -325, 385, -200, 350, 300, 80, 14, true, 300);
            CreateInfo("While sticking to a surface \n the up key can detatch you at any time.", -450, 625, -500, 600, 300, 80, 14, true, 300);
            CreateInfo("To flip switches run into them.", 0, 625, 0, 500, 300, 80, 14, true, 200);
            CreateInfo("/" + "\\" + "\n|", 200, 650, 300, 500, 300, 80, 14, true, 100);
            CreateInfo("While in air you can use \n W/Up A/Left S/Down D/Right \n to propel yourself!", 500, -75, 530, -200, 300, 80, 14, true, 200);
            CreateInfo("Careful though propulsion has a price!" + "\n" + "The green bar at the bottom shows how much you can use.", 1000, 50, 900, 0, 400, 80, 14, true, 200);
            CreateInfo("You can refill this bar by touching green surfaces.", 1450, 50, 1200, 0, 400, 80, 14, true, 200);
            CreateInfo("Moving platforms. make sure not to fall off!", 2000, -1075, 2000, -1200, 400, 80, 14, true, 200);

            CreateInfo("Pressure Plates! Hop on!" + "\n" + "->", 3400, -1900, 3200, -2100, 400, 80, 14, true, 200);

            CreateInfo("Sometimes you have to be patient.", 4100, -1780, 4200, -1850, 400, 80, 14, true, 200);

            CreateInfo("Sometimes you have to rush it!", 5000, -1780, 5600, -1780, 400, 80, 14, true, 800);

            CreateInfo("Don't fall!" + "\n" + "|" + "\n" + "\\/", 4950, -3990, 4900, -4100, 400, 80, 14, true, 200);

            CreateInfo("Need a lift?", 2880, -4120, 2880, -3900, 100, 20, 14, true, 400);

            CreateInfo("Maybe a push?", 3400, -5075, 3400, -5150, 100, 20, 14, true, 300);

            CreateInfo("You got your goo all over my pressure plate!" + "\n" + "Now it's stuck!", 3925, -5300, 4000, -5280, 500, 40, 14, true, 200);

            CreateInfo("Really the airvent? Your cheesy.", 5200, -5450, 5300, -5450, 400, 40, 14, true, 400);

            level = 1;
            //create all the collision lines
            CreateLine(-400, -200, -400, 700, true, true);
            CreateLine(-400, -200, 800, -200, false, true);
            CreateLine(-200, 200, 200, 200, true, true);
            CreateLine(-200, 200, -200, 300, false, true);
            CreateLine(-400, 700, 400, 700, true, true);
            CreateLine(400, 700, 400, 0, true, true);
            CreateLine(400, 0, 800, 0, true, true);
            CreateLine(800, 0, 800, 500, true, true);
            CreateLine(1600, 500, 800, 500, false, true);
            CreateLine(1600, 500, 1600, -500, true, false);
            CreateLine(1600, -500, 2980, -500, true, false);

            CreateLine(800, -200, 800, -400, false, false);
            CreateLine(800, -400, 1430, -400, false, false);
            CreateLine(1430, -400, 1430, -1500, false, true);
            CreateLine(1430, -1500, 2600, -1500, false, false);
            CreateLine(2600, -1500, 2600, -2200, false, true);
            CreateLine(2600, -2200, 3900, -2200, false, true);

            CreateLine(2980, -1850, 3900, -1850, true, true);
            CreateLine(2980, -1850, 2980, -500, false, false);
            CreateLine(3900, -1850, 3900, -1700, true, false);
            CreateLine(3900, -1700, 4300, -1700, true, false);
            CreateLine(4300, -1000, 6000, -1000, true, false);
             CreateLine(4300, -1700, 4300, -1000, true, true);

            CreateLine(3900, -2200, 6000, -2200, false, false);
            CreateLine(6000, -1000, 6000, -1700, true, false);
            CreateLine(6000, -1700, 6500, -1700, true, false);

            CreateLine(6500, -1700, 6500, -3960, true, false);
            CreateLine(6000, -2200, 6000, -3500, false, false);

            CreateLine(6000, -3700, 6000, -3990, false, false);



            CreateLine(5010, -4000, 6000, -4000, true, false);
            CreateLine(5010, -4200, 5450, -4200, false, false);
            CreateLine(5500, -4200, 6500, -4200, false, false);
            CreateLine(5450, -4200, 5450, -5350, false, false);


            CreateLine(5010, -4200, 5010, -4300, false, false);
            CreateLine(3440, -4300, 5010, -4300, false, false);
            CreateLine(3440, -4200, 3440, -4300, false, false);

            CreateLine(5010, -3500, 6000, -3500, true, false);
            CreateTypeLine(3440, -3500, 5010, -3500, true, false, 6, -1, -1, -1);
            CreateLine(6000, -3700, 6000, -4000, false, false);
            CreateLine(5010, -3700, 6000, -3700, false, false);

            CreateLine(3440, -4000, 3010, -4000, false, false);

            CreateLine(5010, -4000, 5010, -3700, false, false);
            CreateLine(3440, -3500, 3440, -4000, false, false);

            CreateLine(3010, -3800, 3010, -4000, true, false);
            CreateLine(2980, -4200, 3440, -4200, false, false);
            CreateLine(3010, -3800, 2980, -3800, false, false);
            CreateLine(2980, -3800, 2980, -3970, false, false);
            CreateLine(2780, -3970, 2980, -3970, true, false);
            CreateLine(2980, -4200, 2980, -5000, true, false);
            CreateLine(2780, -3970, 2780, -5200, false, false);
            CreateLine(2780, -5200, 2980, -5200, false, false);
            CreateLine(2980, -5000, 3500, -5000, true, false);
            CreateLine(2980, -5200, 2980, -5500, false, false);
            CreateLine(2980, -5500, 4000, -5500, false, false);
            CreateLine(3500, -5000, 3500, -4900, true, false);
            CreateLine(3500, -4900, 3700, -4900, true, false);
            CreateLine(3700, -4900, 3700, -5350, true, false);
            CreateLine(3700, -5350, 4000, -5350, true, false);
            CreateLine(4000, -5400, 4000, -5500, true, false);
            CreateLine(4000, -5350, 5450, -5350, true, false);
            CreateLine(5500, -5400, 5500, -4200, false, false);
            CreateLine(4000, -5400, 5500, -5400, false, false);
            CreateLine(6500, -4200, 6500, -4400, false, false);
            CreateLine(6500, -4400, 6520, -4400, false, false);
            CreateLine(6520, -4400, 6520, -4200, false, false);
            CreateLine(6500, -3960, 7000, -3960, true, false);
            CreateLine(6520, -4200, 7100, -4200, false, false);
            CreateLine(7000, -3960, 7000, -4000, true, false);
            CreateLine(7000, -4000, 7100, -4000, true, false);
            CreateLine(7100, -4000, 7100, -3980, true, false);
            CreateLine(7100, -3980, 7300, -3980, true, false);
            CreateLine(7300, -3980, 7300, -7000, true, false);
            CreateLine(7100, -4200, 7100, -7000, false, false);


            CreateLine(200, 200, 200, 300, true, true);
            CreateLine(-200, 300, 200, 300, false, true);

            CreateBlock(1100, 150, 200, 100, true, true, true, true);
            CreateBlock(1450, 50, 50, 400, true, true, true, true);
            CreateBlock(1600, -1200, 100, 30, true, true, true, true);
            CreateBlock(2000, -1000, 200, 50, true, true, true, true);
            CreateBlock(2800, -980, 200, 40, true, true, true, true);

            int size = collides.Count;
            for (int i = 0; i < size; i++)
            {
                for (int t = 0; t < size; t++)
                {
                    if (i != t)
                    {
                        ConnectTwoPoints(i, t);
                    }
                }
            }

            CreateDoor(3900, -2200, 3900, -1850, false, false, createButton(3400, -1850, false), 4, 2, false);
            CreateDoor(4000, -1700, 6000, -1700, true, false, createButton(4100, -1700, false), 6, 1, true);
            CreateDoor(6010, -1700, 6010, -4000, false, true, createButton(6100, -1700, false), 6, 1, true);
            CreateMovingPlatform(2100, -980, 100, 20, 2200, -980, 2600, -980, true, 1, 60);
            CreateMovingPlatform(3000, -980, 20, 200, 2940, -1200, 2940, -1670, true, 1, 60);
            int mySwitch = CreateSwitch(4750, -3950, true);
            CreateMovingLine(4950, -3950, 4950, -3990
                    , 4950 - 1000, -3950, 4950 - 1000, -3990, true, true, 100, mySwitch, false);

            CreateMovingLine(4950, -3990, 4960, -4000
                    , 4950 - 1000, -3990, 4960 - 1000, -4000, true, true, 100, mySwitch, false);

            CreateMovingLine(4960, -4000, 4990, -4000
                 , 4960 - 1000, -4000, 4990 - 1000, -4000, true, true, 100, mySwitch, false);

            CreateMovingLine(4990, -4000, 5000, -3990
                 , 4990 - 1000, -4000, 5000 - 1000, -3990, true, true, 100, mySwitch, false);

            CreateMovingLine(5000, -3990, 5000, -3920
                 , 5000 - 1000, -3990, 5000 - 1000, -3920, true, true, 100, mySwitch, false);
            CreateMovingLine(4490, -4000, 4500, -3990
                 , 4490 - 1000, -4000, 4500 - 1000, -3990, true, true, 100, mySwitch, false);
            CreateMovingLine(4500, -3990, 4500, -3950
               , 4500 - 1000, -3990, 4500 - 1000, -3950, true, true, 100, mySwitch, false);

            CreateMovingLine(4460, -4000, 4490, -4000
                 , 4460 - 1000, -4000, 4490 - 1000, -4000, true, true, 100, mySwitch, false);

            CreateMovingLine(4450, -3990, 4450, -3920
                 , 4450 - 1000, -3990, 4450 - 1000, -3920, false, true, 100, mySwitch, false);

            CreateMovingLine(4450, -3990, 4460, -4000
                 , 4450 - 1000, -3990, 4460 - 1000, -4000, true, true, 100, mySwitch, false);

            CreateMovingLine(4500, -3950, 4950, -3950
                , 4500 - 1000, -3950, 4950 - 1000, -3950, true, true, 100, mySwitch, true);

            CreateMovingLine(4450, -3920, 5000, -3920
                , 4450 - 1000, -3920, 5000 - 1000, -3920, false, true, 100, mySwitch, false);

            int power = CreateMovingBlock(2995, -3900, 10, 100, 5, 0, -200, false, 200, CreateSwitch(2880, -3990, false), false);
            CreateMovingBlock(2880, -3980, 100, 10, 5, 0, -1010, false, 200, power, true);
            int myPower = CreateElectrical(4000, -4990, collides.Count + 2);
            CreateMovingBlock(3600, -4990, 100, 10, 5, 0, 70, false, 8, myPower, true);
            int buttonIndex = createButton(3850, -5350, true);

            int index2 = CreateMovingBlock(6750, -3990, 250, 10, 5, -500, 0, false, 100, buttonIndex, false);
            CreateMovingBlock(6510, -4100, 10, 100, 5, 0, -200, false, 100, buttonIndex, false);
            CreateMovingBlock(6750, -3970, 250, 10, 5, 0, -20, false, 100, index2, false);
            CreateMovingBlock(410, -100, 10, 100, 5, 0, -200, true, 200, CreateSwitch(200,700,false), false);
            finalSwitch = CreateSwitch(7200, -4000, false);
            CreateMovingBlock(7200, -3990, 100, 10, 5, 0, -3000, false, 200, finalSwitch, true);
        }

        //utility functions
        public double toRadians(double d)
        {
            return (d * Math.PI / 180.0);
        }
        public double toDegrees(double r)
        {
            return (r * 180.0 /  Math.PI);
        }
        private double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        private double PerpVec(Vector v1, Vector v2)
        {
            Vector tmp1 = new Vector(v1.X, v1.Y);
            Vector tmp2 = new Vector(-v2.Y, v2.X);
            tmp2.Normalize();
            return (tmp1.X * tmp2.X + tmp1.Y * tmp2.Y);
        }
        private void SetUpLines()
            {
            level = 2;

            CreateInfo("Looks like you broke the switch", -900, 2750, -800, 2700, 200, 40, 14, true, 200);
            CreateInfo("A slide!", 0, 150, 100, -100, 200, 40, 14, true, 300);
            CreateInfo("More THRUST!", 0, 2650, -100, 2400, 200, 40, 14, true, 300);
            CreateInfo("You ready to be an astronaut?", 5100, -3400, 5100, -4200, 200, 40, 14, true, 900);
            CreateInfo("Zero-G. You got no pull,\nno yin for yang!", 5100, -5100, 5000, -5100, 200, 40, 14, true, 200);
            CreateInfo("<-", 5200, -5270, 5000, -5270, 40, 40, 20, true, 400);

            CreateInfo("Careful with your propusion!\nGot Stuck?\nPress R to return to the last safe surface.", 6100, -3250, 6000, -3200, 300, 60, 14, true, 300);

            CreateInfo("Blue Surfaces change your gravity!", 8000, 0, 7800, 0, 300, 60, 14, true, 300);
            CreateLine(-400, 200, 400, 200, true, false);
                
            float dir = -90;
            int addX = 1400;
            int addY = 200;
            float size2X = 1000;
            float size2Y = 2000;

                for(int i = 0; i < 20; i ++)
            {
                float x1 = (float)Math.Sin(toRadians(dir)) * size2X;
                float y1 = (float)Math.Cos(toRadians(dir)) * size2Y;
                dir = dir + 5;
                float x2 = (float)Math.Sin(toRadians(dir)) * size2X;
                float y2 = (float)Math.Cos(toRadians(dir)) * size2Y;
                CreateLine((int)Math.Round(x1) + addX, (int)Math.Round(y1) + addY, (int)Math.Round(x2) + addX, (int)Math.Round(y2) + addY, true, false);

            }
            float x3 = (float)Math.Sin(toRadians(dir)) * size2X;
            float y3 = (float)Math.Cos(toRadians(dir)) * size2Y;
            CreateLine((int)Math.Round(x3) + addX, (int)Math.Round(y3) + addY, 1600, 2300, true, false);
            CreateLine(1600, 2300, -400, 2700, true, false);

            CreateLine(-400, 2800, 3000, 2300, true, false);

            CreateLine(-1000, 2800, -525, 2800, true, false);
            CreateLine(-600, 2700, -1200, 2700, true, false);
            CreateLine(-1200, 2700, -1200, 4000, true, false);
            CreateLine(-1000, 2800, -1000, 4000, false, false);

            CreateLine(-475, 2800, -400, 2800, true, false);
            CreateLine(-475, 2800, -475, 2725, false, true);
            CreateLine(-525, 2725, -475, 2725, true, false);
            CreateLine(-525, 2725, -525, 2800, false, true);
            

            CreateLine(3000, 2300, 3000, 2000, true, false);
            CreateLine(3000, 2000, 4000, 2000, true, false);
            CreateLine(4000, 2000, 4000, 0, true, true);
            CreateLine(4000, 0, 5000, 0, true, false);
            CreateLine(5000, 0, 5000, 40, true, false);
            CreateLine(5000, 40, 5200, 40, true, false);
            CreateLine(5200, 40, 5200, 0, true, false);
            CreateLine(5200, 0, 6000, 0, true, false);
            CreateLine(6000, -100, 6000, -700, true, false);
            CreateLine(6000, -100, 7000, -100, false, false);
            CreateLine(6000, 0, 7000, 0, true, false);
            CreateGravityLine(8100, 400, 8100, -500, true, 0.1f);
            CreateLine(7000, 0, 7000, 400, true, true);
            CreateLine(7000, 400, 8100, 400, true, true);


            

            CreateLine(7000, -100, 7000, -800, false, false);
            CreateLine(7000, -800, 8800, -800, false, false);
            CreateTypeLine(8800, -800, 8800, -300, false, false, 6, -1, -1, -1);


            CreateLine(8100, -500, 8200, -500, true, false);
            CreateLine(8200, -500, 8200, -450, true, false);
            CreateGravityLine(8200, -450, 8500, -450, true, 0.1f);

            CreateLine(8500, -450, 8500, 1000, true, true);
            CreateGravityLine(8800, -300, 8800, 600, false, 0.1f);

            CreateLine(8800, 600, 9150, 600, false, false);

            CreateGravityLine(9275, 600, 9275, 200, true, 0.1f);
            CreateLine(9150, 600, 9200, 100, false, false);
            CreateLine(9275, 600, 9400, 600, false, false);
            CreateTypeLine(9400, 1000, 9400, 600, true, false, 6, -1, -1, -1);
            CreateLine(8500, 1000, 9400, 1000, true, true);
            CreateLine(9275, 200, 9425, 200, true, false);
            CreateLine(9275, 100, 9275, -6000, false, false);
            CreateLine(9200, 100, 9275, 100, false, false);
            CreateLine(9425, 200, 9425, -6000, true, false);





            CreateLine(6000, -700, 5200, -700, true, false);
            CreateLine(5200, -700, 5200, -3000, true, false);
            CreateLine(5000, -3000, 5000, -700, true, false);
            
            CreateLine(-600, -700, 5000, -700, false, false);
            CreateLine(-600, -700, -600, 200, true, false);

            CreateLine(-400, 2700, -400, 200, true, true);
            CreateLine(-600, 2700, -600, 200, false, true);

            CreateLine(5000, -3000, 4000, -3000, false, true);
            CreateLine(5200, -3000, 6200, -3000, true, true);
            CreateLine(4000, -3000, 4000, -5200, false, true);
            CreateLine(6200, -3300, 6200, -5200, true, true);
            CreateLine(4000, -5200, 6200, -5200, false, true);
            CreateLine(6200, -3100, 8000, -3100, true, false);
            CreateLine(6200, -3300, 7800, -3300, false, false);
            CreateLine(6200, -3000, 6200, -3100, true, true);
            CreateLine(8000, -3100, 8000, -6500, true, false);
            CreateLine(6000, -6500, 8000, -6500, false, false);
            CreateLine(6000, -6300, 7800, -6300, true, false);
            CreateLine(7800, -3300, 7800, -6300, false, false);
            CreateLine(5800, -6500, 5800, -7000, false, false);
            CreateLine(6000, -6500, 6000, -7000, true, true);
            CreateLine(6000, -7000, 6500, -7000, true, true);
            CreateLine(5300, -7000, 5800, -7000, true, true);
            CreateLine(5300, -7000, 5300, -7300, false, true);
            CreateLine(5300, -7300, 6000, -7300, false, true);
            CreateLine(6000, -7300, 6000, -7400, false, true);
            CreateLine(6500, -7000, 6500, -7600, true, false);
            CreateLine(6500, -7600, 5500, -7600, true, false);
            CreateLine(5500, -7400, 6000, -7400, true, true);
            CreateLine(5500, -7400, 5500, -7600, false, false);


            int size = collides.Count;
                for (int i = 0; i < size; i++)
                {
                    for (int t = 0; t < size; t++)
                    {
                        if (i != t)
                        {
                            ConnectTwoPoints(i, t);
                        }
                    }
                }

             dir = 0;
             addX = 6000;
             addY = -6500;
             size2X = 200;
             size2Y = 200;

            for (int i = 0; i < 20; i++)
            {
                float x1 = (float)Math.Sin(toRadians(dir)) * size2X;
                float y1 = (float)Math.Cos(toRadians(dir)) * size2Y;
                dir = dir - 4.5f;
                float x2 = (float)Math.Sin(toRadians(dir)) * size2X;
                float y2 = (float)Math.Cos(toRadians(dir)) * size2Y;
                CreateLine((int)Math.Round(x1) + addX, (int)Math.Round(y1) + addY, (int)Math.Round(x2) + addX, (int)Math.Round(y2) + addY, false, false);

            }

            CreateMovingBlock(5100, 20, 100, 20, 5, 0, -3000, true, 200, CreateSwitch(5100, 0, false), true);
            int myPower = CreateElectrical(4000, -4990, collides.Count + 2);
            CreateMovingBlock(8050, -3200, 20, 100, 5, -150, 0, false, 10, myPower, true);
            int myPower2 = CreateElectrical(4000, -4990, collides.Count + 2);
            CreateMovingBlock(8050, -6400, 20, 100, 5, -150, 0, false, 10, myPower2, true);

            int myPower3 = CreateElectrical(4000, -4990, collides.Count + 2);
            CreateMovingBlock(5900, -7250, 100, 20, 5, 0, 150, false, 10, myPower3, true);



            int power4 = CreateSwitch(5700, -7400, false);
            CreateMovingBlock(6010, -50, 10, 50, 5, 0, 100, false, 10, power4, false);

            CreateMovingBlock(-1100, 2820, 100, 20, 5, 0, 5, false, 10,CreateSwitch(-1100,2800,false) , true);



        }
        public int MovingLine(int x1, int y1, int x2, int y2,int moveAmountX,int moveAmountY, int repeats, int powerIndex, bool movePower, bool flip, bool sticks)
        {
            CreateMovingLine(x1, y1, x2, y2,
                x1 + moveAmountX, y1 + moveAmountY, x2 + moveAmountX, y2 + moveAmountY, flip, sticks, repeats, powerIndex, movePower);
            int index = collides.Count - 1;
            return index;
        }
        public int CreateElectrical(int x, int y,int toPower)
        {
            CreateTypeLine(x, y, x, y, true, false, 5, -1, -1, toPower);
            return collides.Count - 1;
        }
        public int CreateMovingBlock(int x, int y, int xSize, int ySize, int dia, int moveAmountX, int moveAmountY, bool sticks, int repeats, int powerIndex, bool movePower)
        {
            CreateMovingLine(x - xSize, y - ySize + dia, x - xSize, y + ySize - dia,
                x - xSize + moveAmountX, y - ySize + moveAmountY + dia, x - xSize + moveAmountX, y + ySize + moveAmountY - dia, false, sticks, repeats, powerIndex, movePower);

            int index = collides.Count - 1;

            CreateMovingLine(x - xSize, y + ySize - dia, x - xSize + dia, y + ySize,
                x - xSize + moveAmountX, y + ySize + moveAmountY - dia, x - xSize + moveAmountX + dia, y + ySize + moveAmountY, false, sticks, repeats, powerIndex, false);


            CreateMovingLine(x - xSize + dia, y + ySize, x + xSize - dia, y + ySize,
                x - xSize + moveAmountX + dia, y + ySize + moveAmountY, x + xSize + moveAmountX - dia, y + ySize + moveAmountY, false, sticks, repeats, powerIndex, false);

            CreateMovingLine(x + xSize - dia, y + ySize, x + xSize, y + ySize - dia,
                x + xSize + moveAmountX - dia, y + ySize + moveAmountY, x + xSize + moveAmountX, y + ySize + moveAmountY - dia, false, sticks, repeats, powerIndex, false);

            CreateMovingLine(x + xSize, y + ySize - dia, x + xSize, y - ySize + dia,
                x + xSize + moveAmountX, y + ySize + moveAmountY - dia, x + xSize + moveAmountX, y - ySize + moveAmountY + dia, false, sticks, repeats, powerIndex, false);

            CreateMovingLine(x + xSize, y - ySize + dia, x + xSize - dia, y - ySize,
                x + xSize + moveAmountX, y - ySize + moveAmountY + dia, x + xSize + moveAmountX - dia, y - ySize + moveAmountY, false, sticks, repeats, powerIndex, false);


            CreateMovingLine(x + xSize - dia, y - ySize, x - xSize + dia, y - ySize,
                x + xSize + moveAmountX - dia, y - ySize + moveAmountY, x - xSize + moveAmountX + dia, y - ySize + moveAmountY, false, sticks, repeats, powerIndex, false);

            CreateMovingLine(x - xSize + dia, y - ySize, x - xSize, y - ySize + dia,
                x - xSize + moveAmountX + dia, y - ySize + moveAmountY, x - xSize + moveAmountX, y - ySize + moveAmountY + dia, false, sticks, repeats, powerIndex, false);

            return index;
        }

        public void CreateNewBall(int bx, int by, double xVelo, double yVelo, double bBallSize, float gravityX, float gravityY, int type)
        {
            BallCollide bc = new BallCollide();
            bc.xPos = bx;
            bc.yPos = by;
            bc.xVel = xVelo;
            bc.yVel = yVelo;
            bc.spawnPointX = bx;
            bc.spawnPointY = by;
            bc.ballSize = bBallSize;
            bc.type = type;
            bc.gravityX = gravityX;
            bc.gravityY = gravityY;
            Ellipse shape = new Ellipse();
            shape.Width = bBallSize * 2 * zoom;
            shape.Height = bBallSize * 2 * zoom;
            shape.Stroke = Brushes.Black;
            shape.Fill = Brushes.Black;
            shape.StrokeThickness = 2;

            Canvas.SetLeft(shape, (bc.xPos * zoom + (cameraX * zoom)) + Window.Width / 2);
            Canvas.SetTop(shape, (bc.yPos * zoom + (cameraY * zoom)) + Window.Height / 2);
            bc.ball = shape;
            MyCanvas.Children.Add(bc.ball);
            bCollides.Add(bc);
        }
        private void CreateMovingPlatform(int x, int y, int sizeX, int sizeY, int pX1, int pY1, int pX2, int pY2, bool sticks, float moveSpeed, int waitTime)
        {
            Platform p = new Platform();
            p.index1 = collides.Count;
            p.index2 = p.index1 + 1;
            p.index3 = p.index1 + 2;
            p.index4 = p.index1 + 3;
            CreatePlatformLine(x - sizeX, y - sizeY, x - sizeX, y + sizeY, false, true, movingPlatforms.Count);
            CreatePlatformLine(x - sizeX, y + sizeY, x + sizeX, y + sizeY, false, true, movingPlatforms.Count);
            CreatePlatformLine(x + sizeX, y + sizeY, x + sizeX, y - sizeY, false, true, movingPlatforms.Count);
            CreatePlatformLine(x + sizeX, y - sizeY, x - sizeX, y - sizeY, false, true, movingPlatforms.Count);
            p.movePoint1X = pX1;
            p.movePoint1Y = pY1;
            p.movePoint2X = pX2;
            p.movePoint2Y = pY2;
            p.platformSize = new Vector(sizeX, sizeY);
            p.position = new Vector(x, y);
            p.moveSpeed = moveSpeed;
            p.waitTime = waitTime;
            Vector newVel = new Vector(p.position.X - p.movePoint2X, p.position.Y - p.movePoint2Y);
            newVel.Normalize();
            newVel.X = newVel.X * -p.moveSpeed;
            newVel.Y = newVel.Y * -p.moveSpeed;
            p.vel = newVel;
            movingPlatforms.Add(p);
        }
        private void CreateParticle(float x, float y, float xVel, float yVel, byte R, byte G, byte B, float size, float sizeChange)
        {
            Particle p = new Particle();
            p.x = x;
            p.y = y;
            p.xVel = xVel;
            p.yVel = yVel;
            p.size = size;
            p.sizeChange = sizeChange;
            Ellipse shape = new Ellipse();
            shape.Width = size;
            shape.Height = size;
            shape.Fill = Brushes.DarkGreen;
            p.shape = shape;
            MyCanvas.Children.Add(p.shape);
            Canvas.SetLeft(p.shape, (p.x * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2);
            Canvas.SetTop(p.shape, (p.y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2);
            particles.Add(p);

        }
        private void CreateInfo(string content,float x, float y, float slideFromX, float slideFromY, double xSize, double ySize, int fontSize, bool center, float dis)
        {
            InfoText textInfo = new InfoText();
            TextBlock block = new TextBlock();
            block.Width = xSize;
            block.Height = ySize;
            block.FontSize = fontSize;
            if (center)
            {
                block.TextAlignment = System.Windows.TextAlignment.Center;
            }
            block.Opacity = 0;
            block.Text = content;
            MyCanvas.Children.Add(block);
            Canvas.SetZIndex(block, 1);
            textInfo.xPos = x;
            textInfo.yPos = y;
            textInfo.slideFromX = slideFromX;
            textInfo.slideFromY = slideFromY;
            textInfo.xSize = xSize;
            textInfo.ySize = ySize;
            textInfo.content = content;
            textInfo.dis = dis;
            textInfo.textInfo = block;
            Information.Add(textInfo);
            
            
        }
        private void CreateDoor(float x1, float y1, float x2, float y2, bool flip, bool sticks, int powerIndex, float moveSpeed, float moveTo, bool invert)
        {
            CollideLine line = new CollideLine();
            line.MoreData = new float[5];
            
            if (flip == true)
            {
                line.x1 = x1;
                line.y1 = y1;
                line.x2 = x2;
                line.y2 = y2;
                line.MoreData[3] = moveTo;
                if (moveTo == 1)
                {
                    line.MoreData[0] = x1;
                    line.MoreData[1] = y1;
                }
                else if (moveTo == 2)
                {
                    line.MoreData[0] = x2;
                    line.MoreData[1] = y2;
                }


            }
            else
            {
                line.x1 = x2;
                line.y1 = y2;
                line.x2 = x1;
                line.y2 = y1;
                line.MoreData[3] = moveTo;
                if (moveTo == 1)
                {
                    line.MoreData[3] = 2;
                    line.MoreData[0] = x1;
                    line.MoreData[1] = y1;
                }
                else if (moveTo == 2)
                {
                    line.MoreData[3] = 1;
                    line.MoreData[0] = x2;
                    line.MoreData[1] = y2;
                }

            }
            
            Line shape = new Line();
            shape.X1 = x1;
            shape.Y1 = y1;
            shape.X2 = x2;
            shape.Y2 = y2;
            if (sticks == false)
            {
                shape.Stroke = Brushes.Black;
            }
            else
            {
                shape.Stroke = Brushes.DarkGreen;
            }
            shape.StrokeThickness = 2;
            Vector v = new Vector();
            if (flip == false)
            {
                v = new Vector(x1 - x2, y1 - y2);
            }
            else
            {
                v = new Vector(x2 - x1, y2 - y1);
            }
            v.Normalize();
            double tmp = v.X;
            v.X = v.Y * -1;
            v.Y = tmp;
            line.canStick = sticks;
            line.normal.X = v.X;
            line.type = 3;
            line.activatedSpecial = invert;
            line.normal.Y = v.Y;
            
            
            line.MoreData[2] = moveSpeed;
            line.index1Refrence = -1;
            line.index2Refrence = -1;
            line.index3Refrence = powerIndex;
            line.lineShape = shape;
            MyCanvas.Children.Add(line.lineShape);
            line.lastDot.Add(0);
            for (int i = 0; i < bCollides.Count; i++)
            {

                line.lastDot.Add(0);
            }
            collides.Add(line);
        }
        private void CreatePlatformLine(int x1, int y1, int x2, int y2, bool flip, bool sticks, int platformIndex)
        {
            CollideLine line = new CollideLine();
            //this helps determine collision normals
            if (flip == true)
            {
                line.x1 = x1;
                line.y1 = y1;
                line.x2 = x2;
                line.y2 = y2;
            }
            else
            {
                line.x1 = x2;
                line.y1 = y2;
                line.x2 = x1;
                line.y2 = y1;
            }
            Line shape = new Line();
            shape.X1 = x1;
            shape.Y1 = y1;
            shape.X2 = x2;
            shape.Y2 = y2;
            if (sticks == false)
            {
                shape.Stroke = Brushes.Black;
            }
            else
            {
                shape.Stroke = Brushes.DarkGreen;
            }
            shape.StrokeThickness = 2;
            Vector v = new Vector();
            if (flip == false)
            {
                v = new Vector(x1 - x2, y1 - y2);
            }
            else
            {
                v = new Vector(x2 - x1, y2 - y1);
            }
            v.Normalize();
            double tmp = v.X;
            v.X = v.Y * -1;
            v.Y = tmp;
            line.canStick = sticks;
            line.normal.X = v.X;
            line.normal.Y = v.Y;
            line.lineShape = shape;
            line.isMovingPlatform = true;
            line.platformIndex = platformIndex;
            MyCanvas.Children.Add(line.lineShape);
            line.lastDot.Add(0);
            for (int i = 0; i < bCollides.Count; i++)
            {

                line.lastDot.Add(0);
            }
            collides.Add(line);


        }
        public int CreateSwitch(int x, int y, bool invert)
        {
            if (invert)
            {
                CreateTypeLine(x, y, x, y - 30, true, false, 2, -1, -1, -1);
            }
            else
            {
                CreateTypeLine(x, y - 30, x, y, false, false, 2, -1, -1, -1);
            }
            CollideLine c = collides[collides.Count - 1];
            c.MoreData = new float[2];
            if (invert)
            { 
            float tmpX = c.x2;
            float tmpY = c.y2;
            c.x2 = c.x1;
            c.y2 = c.y1;
            c.x1 = tmpX;
            c.y1 = tmpY;
            c.activatedSpecial = false;
            Vector v1 = new Vector();

            v1 = new Vector(c.x2 - c.x1, c.y2 - c.y1);
            v1.Normalize();
            double tmp = v1.X;
            v1.X = v1.Y * -1;
            v1.Y = tmp;
            c.normal.X = v1.X;
            c.normal.Y = v1.Y;
            }
            if (invert)
            {
                c.MoreData[1] = 1;
            }
            else
            {
                c.MoreData[1] = 0;
            }

            return collides.Count - 1;
        }
        public int createButton(int x, int y, bool locks)
        {
            int refrence1 = collides.Count;
            int refrence2 = collides.Count + 1;
            CreateLine(x - 80, y - 15, x - 100, y, false, false);
            CreateLine(x + 100, y, x + 80,y - 15, false, false);
            if (locks)
            {
                CreateTypeLine(x + 80, y - 15, x - 80, y - 15, false, false, 1, refrence1, refrence2, 1);
            }
            else
            {
                CreateTypeLine(x + 80, y - 15, x - 80, y - 15, false, false, 1, refrence1, refrence2, 0);
            }
            return refrence2 + 1;
        }
        public void CreateGravityLine(int x1, int y1, int x2, int y2, bool flip, float strength)
        {
            CollideLine line = new CollideLine();
            line.MoreData = new float[1];
            line.MoreData[0] = strength;
            if (flip == true)
            {
                line.x1 = x1;
                line.y1 = y1;
                line.x2 = x2;
                line.y2 = y2;
            }
            else
            {
                line.x1 = x2;
                line.y1 = y2;
                line.x2 = x1;
                line.y2 = y1;
            }
            Line shape = new Line();
            shape.X1 = x1;
            shape.Y1 = y1;
            shape.X2 = x2;
            shape.Y2 = y2;
            shape.StrokeThickness = 2;
            Vector v = new Vector();
            if (flip == false)
            {
                v = new Vector(x1 - x2, y1 - y2);
            }
            else
            {
                v = new Vector(x2 - x1, y2 - y1);
            }
            v.Normalize();
            double tmp = v.X;
            v.X = v.Y * -1;
            v.Y = tmp;
            line.canStick = false;
            line.normal.X = v.X;
            line.type = 7;
            line.normal.Y = v.Y;
            line.index1Refrence = -1;
            line.index2Refrence = -1;
            line.index3Refrence = -1;
            line.lineShape = shape;
            MyCanvas.Children.Add(line.lineShape);
            line.lastDot.Add(0);
            for (int i = 0; i < bCollides.Count; i++)
            {

                line.lastDot.Add(0);
            }
            collides.Add(line);
        }
        public void CreateTypeLine(int x1, int y1, int x2, int y2, bool flip, bool sticks, int type, int refrence1, int refrence2, int refrence3)
        {
            CollideLine line = new CollideLine();
            if (flip == true)
            {
                line.x1 = x1;
                line.y1 = y1;
                line.x2 = x2;
                line.y2 = y2;
            }
            else
            {
                line.x1 = x2;
                line.y1 = y2;
                line.x2 = x1;
                line.y2 = y1;
            }
            Line shape = new Line();
            shape.X1 = x1;
            shape.Y1 = y1;
            shape.X2 = x2;
            shape.Y2 = y2;
            if (sticks == false)
            {
                shape.Stroke = Brushes.Black;
            }
            else
            {
                shape.Stroke = Brushes.DarkGreen;
            }
            shape.StrokeThickness = 2;
            Vector v = new Vector();
            if (flip == false)
            {
                v = new Vector(x1 - x2, y1 - y2);
            }
            else
            {
                v = new Vector(x2 - x1, y2 - y1);
            }
            v.Normalize();
            double tmp = v.X;
            v.X = v.Y * -1;
            v.Y = tmp;
            line.canStick = sticks;
            line.normal.X = v.X;
            line.type = type;
            line.normal.Y = v.Y;
            line.index1Refrence = refrence1;
            line.index2Refrence = refrence2;
            line.index3Refrence = refrence3;
            line.lineShape = shape;
            MyCanvas.Children.Add(line.lineShape);
            line.lastDot.Add(0);
            for (int i = 0; i < bCollides.Count; i++)
            {

                line.lastDot.Add(0);
            }
            collides.Add(line);
        }
        private void CreateMovingLine(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, bool flip, bool sticks, float repeats, int powerIndex, bool movePower)
        {
            CollideLine line = new CollideLine();
            line.MoreData = new float[13];
            line.MoreData[12] = 0;
            if (flip == true)
            {
                line.x1 = x1;
                line.y1 = y1;
                line.x2 = x2;
                line.y2 = y2;
                line.MoreData[1] = x1;
                line.MoreData[2] = y1;
                line.MoreData[3] = x2;
                line.MoreData[4] = y2;
                line.MoreData[5] = x3;
                line.MoreData[6] = y3;
                line.MoreData[7] = x4;
                line.MoreData[8] = y4;


            }
            else
            {
                line.x1 = x2;
                line.y1 = y2;
                line.x2 = x1;
                line.y2 = y1;
                line.MoreData[1] = x2;
                line.MoreData[2] = y2;
                line.MoreData[3] = x1;
                line.MoreData[4] = y1;
                line.MoreData[5] = x4;
                line.MoreData[6] = y4;
                line.MoreData[7] = x3;
                line.MoreData[8] = y3;


            }

            Line shape = new Line();
            shape.X1 = x1;
            shape.Y1 = y1;
            shape.X2 = x2;
            shape.Y2 = y2;
            if (sticks == false)
            {
                shape.Stroke = Brushes.Black;
            }
            else
            {
                shape.Stroke = Brushes.DarkGreen;
            }
            shape.StrokeThickness = 2;
            Vector v = new Vector();
            if (flip == false)
            {
                v = new Vector(x1 - x2, y1 - y2);
            }
            else
            {
                v = new Vector(x2 - x1, y2 - y1);
            }
            v.Normalize();
            double tmp = v.X;
            v.X = v.Y * -1;
            v.Y = tmp;
            line.canStick = sticks;
            line.normal.X = v.X;
            line.type = 4;
            line.normal.Y = v.Y;
            if (movePower)
            {
                line.MoreData[10] = 1;
            }
            else
            {
                line.MoreData[10] = 0;
            }
            
            line.MoreData[0] = repeats;
            line.index1Refrence = -1;
            line.index2Refrence = -1;
            line.index3Refrence = powerIndex;
            line.lineShape = shape;
            MyCanvas.Children.Add(line.lineShape);
            line.lastDot.Add(0);
            for (int i = 0; i < bCollides.Count; i++)
            {

                line.lastDot.Add(0);
            }
            collides.Add(line);
        }
        private void CreateLine(int x1, int y1, int x2, int y2, bool flip, bool sticks)
        {
            CollideLine line = new CollideLine();
            if (flip == true)
            {
                line.x1 = x1;
                line.y1 = y1;
                line.x2 = x2;
                line.y2 = y2;
            }
            else
            {
                line.x1 = x2;
                line.y1 = y2;
                line.x2 = x1;
                line.y2 = y1;
            }
            Line shape = new Line();
            shape.X1 = x1;
            shape.Y1 = y1;
            shape.X2 = x2;
            shape.Y2 = y2;
            if (sticks == false)
            {
                shape.Stroke = Brushes.Black;
            }
            else
            {
                shape.Stroke = Brushes.DarkGreen;
            }
            shape.StrokeThickness = 2;
            Vector v = new Vector();
            if (flip == false)
            {
                v = new Vector(x1 - x2, y1 - y2);
            }
            else
            {
                v = new Vector(x2 - x1, y2 - y1);
            }
            v.Normalize();
            double tmp = v.X;
            v.X = v.Y * -1;
            v.Y = tmp;
            line.canStick = sticks;
            line.normal.X = v.X;
            line.normal.Y = v.Y;
            line.lineShape = shape;
            MyCanvas.Children.Add(line.lineShape);
            line.lastDot.Add(0);
            for (int i = 0; i < bCollides.Count; i++)
            {

                line.lastDot.Add(0);
            }
            collides.Add(line);

        }
       
        private void ConnectTwoPoints(int index1, int index2)
        {
            CollideLine c1 = collides[index1];
            CollideLine c2 = collides[index2];
            double dis1 = Distance(c1.x1, c1.y1, c2.x1, c2.y1);
            double dis2 = Distance(c1.x1, c1.y1, c2.x2, c2.y2);
            double dis3 = Distance(c1.x2, c1.y2, c2.x1, c2.y1);
            double dis4 = Distance(c1.x2, c1.y2, c2.x2, c2.y2);

            if (dis1 < dis2 && dis1 < dis3 && dis1 < dis4)
            {
                if (dis1 < connectSize + 1)
                {
                    Vector v1 = new Vector(c1.x1 - c1.x2, c1.y1 - c1.y2);
                    Vector v2 = new Vector(c2.x1 - c2.x2, c2.y1 - c2.y2);
                    v1.Normalize();
                    v2.Normalize();
                    Vector point1 = new Vector(c1.x1 + v1.X * -connectSize, c1.y1 + v1.Y * -connectSize);
                    Vector point2 = new Vector(c2.x1 + v2.X * -connectSize, c2.y1 + v2.Y * -connectSize);
                    c1.x1 = (int)Math.Round(point1.X);
                    c1.y1 = (int)Math.Round(point1.Y);
                    c2.x1 = (int)Math.Round(point2.X);
                    c2.y1 = (int)Math.Round(point2.Y);
                    Vector normal = new Vector(point1.X - point2.X, point1.Y - point2.Y);
                    normal.Normalize();
                    double dot = (c1.normal.X * normal.X) + (c1.normal.Y * normal.Y);
                if (dot < 0)
                {
                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, false);
                    }
                }
                else
                {

                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, false);
                    }
                }
                        
                }
            }
            else if (dis2 < dis3 && dis2 < dis4)
            {
                if (dis2 < connectSize + 1)
                {
                    Vector v1 = new Vector(c1.x1 - c1.x2, c1.y1 - c1.y2);
                    Vector v2 = new Vector(c2.x2 - c2.x1, c2.y2 - c2.y1);
                    v1.Normalize();
                    v2.Normalize();
                    Vector point1 = new Vector(c1.x1 + v1.X * -connectSize, c1.y1 + v1.Y * -connectSize);
                    Vector point2 = new Vector(c2.x2 + v2.X * -connectSize, c2.y2 + v2.Y * -connectSize);
                    c1.x1 = (int)Math.Round(point1.X);
                    c1.y1 = (int)Math.Round(point1.Y);
                    c2.x2 = (int)Math.Round(point2.X);
                    c2.y2 = (int)Math.Round(point2.Y);
                    Vector normal = new Vector(point1.X - point2.X, point1.Y - point2.Y);
                    normal.Normalize();
                    double dot = (c1.normal.X * normal.X) + (c1.normal.Y * normal.Y);
                if (dot > 0)
                {
                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, false);
                    }
                }
                else
                {
                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, false);
                    }
                }
                    
            }
            }
            else if (dis3 < dis4)
            {
                if (dis3 < connectSize + 1)
                {
                    Vector v1 = new Vector(c1.x2 - c1.x1, c1.y2 - c1.y1);
                    Vector v2 = new Vector(c2.x1 - c2.x2, c2.y1 - c2.y2);
                    v1.Normalize();
                    v2.Normalize();
                    Vector point1 = new Vector(c1.x2 + v1.X * -connectSize, c1.y2 + v1.Y * -connectSize);
                    Vector point2 = new Vector(c2.x1 + v2.X * -connectSize, c2.y1 + v2.Y * -connectSize);
                    c1.x2 = (int)Math.Round(point1.X);
                    c1.y2 = (int)Math.Round(point1.Y);
                    c2.x1 = (int)Math.Round(point2.X);
                    c2.y1 = (int)Math.Round(point2.Y);
                    Vector normal = new Vector(point1.X - point2.X, point1.Y - point2.Y);
                    normal.Normalize();
                    double dot = (c1.normal.X * normal.X) + (c1.normal.Y * normal.Y);
                if (dot < 0)
                {
                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, false);
                    }
                }
                else
                {
                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, false);
                    }
                }
                    
            }
            }
            else
            {
                if (dis4 < connectSize + 1)
                {
                    Vector v1 = new Vector(c1.x2 - c1.x1, c1.y2 - c1.y1);
                    Vector v2 = new Vector(c2.x2 - c2.x2, c2.y2 - c2.y1);
                    v1.Normalize();
                    v2.Normalize();
                    Vector point1 = new Vector(c1.x2 + v1.X * -connectSize, c1.y2 + v1.Y * -connectSize);
                    Vector point2 = new Vector(c2.x2 + v2.X * -connectSize, c2.y2 + v2.Y * -connectSize);
                    c1.x2 = (int)Math.Round(point1.X);
                    c1.y2 = (int)Math.Round(point1.Y);
                    c2.x2 = (int)Math.Round(point2.X);
                    c2.y2 = (int)Math.Round(point2.Y);
                    Vector normal = new Vector(point1.X - point2.X, point1.Y - point2.Y);
                    normal.Normalize();
                    double dot = (c1.normal.X * normal.X) + (c1.normal.Y * normal.Y);
                if (dot < 0)
                {
                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), true, false);
                    }
                }
                else
                {
                    if (c1.canStick && c2.canStick)
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, true);
                    }
                    else
                    {
                        CreateLine((int)Math.Round(point1.X), (int)Math.Round(point1.Y), (int)Math.Round(point2.X), (int)Math.Round(point2.Y), false, false);
                    }
                }
                    
            }
            }

        }


        //big input function, takes user input and moves him according to the walls and such he is touching, or not touching
        private void StickOnWall()
        {

            //get a vector for the velocity of the player
            Vector speed = new Vector(player.xVel, player.yVel);
            if (speed.Length > 13)
            {
                //normalize the speed vector if we are going so fast
                speed.Normalize();
            }
            //if we aren't touching a surface than we will reset the moving platform velocities
            if (player.touchingIndex == -1)
            {
               //if the player isn't touching a moving platform we will set the additional moving velocity to zero because they aren't on a moving platform
                if (player.movingPlatformTouch != -1)
                {
                    player.addedXVel = 0;
                    player.addedYVel = 0;
                }
                player.movingPlatformTouch = -1;
            }
            //if we are touching a surface we need to do some calculations
            if (player.touchingIndex != -1)
            {
                CollideLine c = collides[player.touchingIndex];
                double ballX = player.xPos + player.ballSize;
                double ballY = player.yPos + player.ballSize;
                Vector cSize = new Vector(c.x1 - c.x2, c.y1 - c.y2);
                Vector cDis1 = new Vector(c.x1 - ballX, c.y1 - ballY);
                Vector cDis2 = new Vector(ballX - c.x2, ballY - c.y2);
                if ((c.type == 0 || c.type == 7) && player.touching && cSize.Length > 11 && cDis1.Length > 75 && cDis2.Length > 75)
                {
                    //if gravity isn't zero g then we don't want to set the last spawn point to a normal surface
                    if (gravity.X != 0 || gravity.Y != 0)
                    {
                        //set up the last safe gravity field spawn spot
                        Vector gravityN = new Vector(gravity.X, -gravity.Y);
                        gravityN.Normalize();
                        double dot = (gravityN.X * c.normal.X + gravityN.Y * c.normal.Y);
                        if(dot > 0.6)
                        {
                            player.lastSafeXPos = player.xPos;
                            player.lastSafeYPos = player.yPos;
                            player.lastSafeGravity.X = gravity.X;
                            player.lastSafeGravity.Y = gravity.Y;
                            player.lastSafeCamRotate = camRotate;
                            player.lastSafeTouchingNormal = player.touchingNormal;
                            player.lastSafeTouchingIndex = player.touchingIndex;
                        }
                    }
                    //if we can stick to the surface it is a good surface in pretty much all cases, in this case all cases for respawning to
                    else if(c.canStick)
                    {
                        player.lastSafeXPos = player.xPos;
                        player.lastSafeYPos = player.yPos;
                        player.lastSafeGravity.X = gravity.X;
                        player.lastSafeGravity.Y = gravity.Y;
                        player.lastSafeCamRotate = camRotate;
                        player.lastSafeTouchingNormal = player.touchingNormal;
                        player.lastSafeTouchingIndex = player.touchingIndex;
                    }
                }
                //if the line we are touching is a moving platform
                if (c.isMovingPlatform)
                {

                    Platform p = movingPlatforms[c.platformIndex];
                    //if the player isn't set to touching a moving platform then make it set to touching one
                    if (player.movingPlatformTouch == -1)
                    {
                        player.movingPlatformTouch = c.platformIndex;

                    }
                    





                    //TODO I think this is actually useless and can be omitted, just double check
                    player.xVel = player.xVel - p.vel.X;
                    player.yVel = player.yVel - p.vel.Y;
                    player.xVel = player.xVel + p.vel.X;
                    player.yVel = player.yVel + p.vel.Y;
                    //if the platform is moving then set the players added vel to something moving
                    if (p.wait < 1)
                    {
                        player.addedXVel = p.vel.X;
                        player.addedYVel = p.vel.Y;
                    }
                    //if the platform isn't moving then set the added velocity to nothing
                    else
                    {
                        if (player.addedXVel != 0 || player.addedYVel != 0)
                        {
                            player.xVel = player.xVel + player.addedXVel;
                            player.yVel = player.yVel - player.addedYVel;
                            player.addedXVel = 0;
                            player.addedYVel = 0;
                        }
                    }


                }
                else
                //if the platform isn't a moving platform then set the player added velocity to nothing and set that they aren't touching a moving platform
                {
                    if (player.movingPlatformTouch != -1)
                    {
                        player.addedXVel = 0;
                        player.addedYVel = 0;

                    }
                    player.movingPlatformTouch = -1;
                }
            }

         //update the camera rotation, based on if they are sticking to a wall
            
            if(camRotate > 180)
            {
                camRotate = -180 + (camRotate - 180);
            }
            if(camRotate < -180)
            {
                camRotate = 180 + (camRotate + 180);
            }
            //do some calculations to figure out if the camera should rotate left or right, its a big chunk of code, due to checking if the direction it needs to rotate is close even if its at -170 and needs to rotate to 180
            if(rotateTo < 0 && 0 < camRotate)
            {
                double point180 = 180 - (-180 - rotateTo);
                double closerDir = camRotate - point180;
                double closerDir2 = camRotate - rotateTo;
                if(Math.Abs(closerDir) < Math.Abs(closerDir2))
                {
                    camRotate = ((point180 - camRotate) / 20) + camRotate;
                    CamDis = (float)(point180 - camRotate);
                }
                else
                {
                    camRotate = ((rotateTo - camRotate) / 20) + camRotate;
                    CamDis = (float)(rotateTo - camRotate);
                }
            }
            else if(0 < rotateTo && camRotate < 0)
            {
                double point180 = -180 - (180 - rotateTo);
                double closerDir = camRotate - point180;
                double closerDir2 = camRotate - rotateTo;
                if(Math.Abs(closerDir) < Math.Abs(closerDir2))
                {
                    camRotate = ((point180 - camRotate) / 20) + camRotate;
                    CamDis = (float)(point180 - camRotate);
                }
                else
                {
                    camRotate = ((rotateTo - camRotate) / 20) + camRotate;
                    CamDis = (float)(rotateTo - camRotate);
                }
            }
            else
            {
                camRotate = ((rotateTo - camRotate) / 20) + camRotate;
                CamDis = (float)(rotateTo - camRotate);
            }
            
            //if player is touching a sticky surface
            if (player.touching && canStick)
            {

                //set the direction that the camera needs to rotate to
                float dir = (float)toDegrees(Math.Acos(player.touchingNormal.Y));
                if (player.touchingNormal.X < 0)
                {
                    dir = dir * -1;

                }
                rotateTo = dir;

                //give the player some more sticky boost
                player.stickyBoost = player.stickyBoost + 2;
                if(player.stickyBoost > player.maxBoost)
                {
                    player.stickyBoost = player.maxBoost;
                }
                attatchedCoolDown = 3;
                /**
                if (Math.Abs(touchingNormal.X) > Math.Abs(touchingNormal.Y))
                {
                    if (touchingNormal.X > 0)
                    {
                        stickingOn = 1;
                        attatchedCoolDown = 3;
                    }
                    else if (touchingNormal.X < 0)
                    {
                        stickingOn = 2;
                        attatchedCoolDown = 3;
                    }
                }
                else
                {
                    if (touchingNormal.Y < 0)
                    {
                        stickingOn = 3;
                        attatchedCoolDown = 3;
                    }
                    else if ( touchingNormZZal.Y > 0)
                    {
                        stickingOn = 0;
                        attatchedCoolDown = 3;
                    }
                }
            */

            }
            //if the player is touching a regular surface
            else if(player.touching)
            {   
                //if the gravity isn't zero g then set the direction the camera needs to rotate to
                if (gravity.X != 0 || gravity.Y != 0)
                {
                    Vector gravityN = new Vector(gravity.X, -gravity.Y);
                    gravityN.Normalize();
                    float dir = (float)toDegrees(Math.Acos(gravityN.Y));
                    if (gravityN.X < 0)
                    {
                        dir = dir * -1;

                    }
                    rotateTo = dir;
                }
            }
            //a cooldown for hopping off of a surface
            if (attatchedCoolDown > 0)
            {
                attatchedCoolDown = attatchedCoolDown - 1;
                if (keyDown == true)
                {
                    
                }

                if (keyUp == true)
                {
                    if (player.liftedUpSinceTouched == 0)
                    {
                        player.yVel = player.yVel + (float)Math.Cos(toRadians(camRotate)) * 0.3;
                        player.xVel = player.xVel - (float)Math.Sin(toRadians(camRotate)) * 0.3;
                        attatchedCoolDown = 0;
                    }
                }
                else
                {
                    //TODO this little dir part could probably be omitted, test code when ready
                    float dir = (float)toDegrees(Math.Acos(player.touchingNormal.Y));
                    if (player.touchingNormal.X < 0)
                    {
                        dir = dir * -1;

                    }
                    if (Math.Round(CamDis) < 5)
                    {
                        player.liftedUpSinceTouched = 0;
                    }
                }
                //move the player based off of input and direction of the camera
                if (keyRight == true)
                {
                    player.xVel = player.xVel + (float)Math.Cos(toRadians(camRotate)) * 0.1;
                    player.yVel = player.yVel + (float)Math.Sin(toRadians(camRotate)) * 0.1;
                    
                }
                else if (keyLeft == true)
                {
                    player.xVel = player.xVel - (float)Math.Cos(toRadians(camRotate)) * 0.1;
                    player.yVel = player.yVel - (float)Math.Sin(toRadians(camRotate)) * 0.1;
                    
                }
                else if (friction)
                {
                    player.yVel = player.yVel * 0.98;
                }
                player.xVel = player.xVel + (float)Math.Sin(toRadians(rotateTo)) * 0.1;
                player.yVel = player.yVel - (float)Math.Cos(toRadians(rotateTo)) * 0.1;




                //TODO looks like this could be omitted
                if (player.stickingOn == 1)
                {

                   // xVel = xVel + 0.1;
                    
                }
                else if (player.stickingOn == 2)
                {
                   // xVel = xVel - 0.1;
                    /**
                    if (keyRight == true)
                    {
                        xVel = xVel + 0.05;
                        attatchedCoolDown = 0;
                    }

                    if (keyLeft == true)
                    {
                        //xVel = -0.05;

                    }
                    if (keyUp == true)
                    {
                        yVel = yVel + 0.1;
                    }
                    else if (keyDown == true)
                    {
                        yVel = yVel - 0.1;
                    }
                    else if (friction)
                    {
                        yVel = yVel * 0.98;
                    }
                */
                }
                else if (player.stickingOn == 3)
                {
                   // yVel = yVel + 0.1;
                    /**
                    if (keyRight == true)
                    {
                        xVel = xVel - 0.05;

                    }
                    else if (keyLeft == true)
                    {
                        xVel = xVel + 0.05;

                    }
                    else if(friction)
                    {
                        xVel = xVel * 0.98;
                    }
                    if (keyUp == true)
                    {
                        //yVel = yVel + 0.1;
                    }
                    else if (keyDown == true)
                    {
                        yVel = yVel - 0.1;
                        attatchedCoolDown = 0;
                    }
                */

                }
                else if (player.stickingOn == 0)
                {
                   // yVel = yVel - 0.1;
                    /**
                    if (keyRight == true)
                    {
                        xVel = xVel + 0.05;

                    }
                    else if (keyLeft == true)
                    {
                        xVel = xVel - 0.05;

                    }
                    else if (friction)
                    {
                        xVel = xVel * 0.98;
                    }
                    if (keyUp == true)
                    {
                        yVel = yVel + 0.1;
                        attatchedCoolDown = 0;
                    }
                    else if (keyDown == true)
                    {
                       // yVel = -0.1;
                    }
                    */

                }
                    

            }
            else // player attached cooldown is === 0, or just less than ya know
            {
                if (player.touching)
                {
                    //move the player if they are touching ground
                    if (keyRight == true)
                    {
                        player.xVel = player.xVel + (float)Math.Cos(toRadians(camRotate)) * 0.1;
                        player.yVel = player.yVel + (float)Math.Sin(toRadians(camRotate)) * 0.1;

                    }
                    else if (keyLeft == true)
                    {
                        player.xVel = player.xVel - (float)Math.Cos(toRadians(camRotate)) * 0.1;
                        player.yVel = player.yVel - (float)Math.Sin(toRadians(camRotate)) * 0.1;

                    }
                }

                player.yVel = player.yVel + gravity.Y;
                player.xVel = player.xVel + gravity.X;

            }
            //if the player can do some mid air sticky boost stuff and he isn't touching the ground
            if (!player.touching && player.stickyBoost > 0)
            { 
                if (keyLeft)
                {
                    player.xVel = player.xVel - (float)Math.Cos(toRadians(camRotate)) * 0.075;
                    player.yVel = player.yVel - (float)Math.Sin(toRadians(camRotate)) * 0.075;
                    player.stickyBoost--;
                    attatchedCoolDown = 0;
                    CreateParticle((float)player.xPos + (float)(player.ballSize * zoom), (float)player.yPos + (float)(player.ballSize * zoom), (float)(r.NextDouble() * 2) - 1f + 3 + (float)player.xVel, (float)(r.NextDouble() * 2) - 1f - (float)player.yVel, 1, 1, 1, r.Next(5) + 12, (float)(r.NextDouble() * -0.6) - 0.2f);
                }
                if (keyRight)
                {
                    player.stickyBoost--;
                    player.xVel = player.xVel + (float)Math.Cos(toRadians(camRotate)) * 0.075;
                    player.yVel = player.yVel + (float)Math.Sin(toRadians(camRotate)) * 0.075;
                    attatchedCoolDown = 0;
                    CreateParticle((float)player.xPos + (float)(player.ballSize * zoom), (float)player.yPos + (float)(player.ballSize * zoom), (float)(r.NextDouble() * 2) - 1f - 3 + (float)player.xVel, (float)(r.NextDouble() * 2) - 1f - (float)player.yVel, 1, 1, 1, r.Next(5) + 12, (float)(r.NextDouble() * -0.6) - 0.2f);
                }
                if (keyDown)
                {
                    player.xVel = player.xVel + (float)Math.Sin(toRadians(camRotate)) * 0.075;
                    player.yVel = player.yVel - (float)Math.Cos(toRadians(camRotate)) * 0.075;
                    player.stickyBoost--;
                    CreateParticle((float)player.xPos + (float)(player.ballSize * zoom), (float)player.yPos + (float)(player.ballSize * zoom), (float)(r.NextDouble() * 2) - 1f + (float)player.xVel, (float)(r.NextDouble() * 2) - 1f - (float)player.yVel - 3, 1, 1, 1, r.Next(5) + 12, (float)(r.NextDouble() * -0.6) - 0.2f);
                    attatchedCoolDown = 0;
                }
                if (keyUp)
                {
                    player.liftedUpSinceTouched = 1;
                    player.xVel = player.xVel - (float)Math.Sin(toRadians(camRotate)) * 0.075;
                    player.yVel = player.yVel + (float)Math.Cos(toRadians(camRotate)) * 0.075;
                    player.stickyBoost--;
                    CreateParticle((float)player.xPos + (float)(player.ballSize * zoom), (float)player.yPos + (float)(player.ballSize * zoom), (float)(r.NextDouble() * 2) - 1f + (float)player.xVel, (float)(r.NextDouble() * 2) - 1f - (float)player.yVel + 3, 1, 1, 1, r.Next(5) + 12, (float)(r.NextDouble() * -0.6) - 0.2f);
                    attatchedCoolDown = 0;
                }
            }
            


        }


        private void UpdatePlayerAndBall()
        {
            //check the players collision with other ball elements
            double ballX = player.xPos + player.ballSize;
            double ballY = player.yPos + player.ballSize;
            for (int i = 0; i < bCollides.Count; i++)
            {
                BallCollide b = bCollides[i];
                double ballX2 = b.xPos + b.ballSize;
                double ballY2 = b.yPos + b.ballSize;
                double dis = Distance(ballX, ballY, ballX2, ballY2);
                if (dis < player.ballSize + b.ballSize && b.touchCount < 2 && player.touching == true)
                {
                    Vector toPos = new Vector(ballX2 - ballX, ballY2 - ballY);
                    toPos.Normalize();
                    toPos.X = toPos.X * (player.ballSize + b.ballSize);
                    toPos.Y = toPos.Y * (player.ballSize + b.ballSize);
                    ballX2 = ballX + toPos.X;
                    ballY2 = ballY + toPos.Y;
                    b.xPos = ballX2 - b.ballSize;
                    b.yPos = ballY2 - b.ballSize;
                    b.xVel = player.xVel;
                    b.yVel = player.yVel;
                }
                else if (dis < player.ballSize + b.ballSize)
                {
                    Vector toPos = new Vector(ballX2 - ballX, ballY2 - ballY);
                    toPos.Normalize();
                    player.yVel = player.yVel * -1;
                    double dot = (player.xVel * toPos.X + (player.yVel) * toPos.Y);
                    player.xVel = player.xVel - (2 * (dot * toPos.X));
                    player.yVel = player.yVel - (2 * (dot * toPos.Y));
                    player.yVel = player.yVel * -0.7;
                    player.xVel = player.xVel * 0.7;

                    toPos.X = toPos.X * (player.ballSize + b.ballSize + 1);
                    toPos.Y = toPos.Y * (player.ballSize + b.ballSize + 1);
                    ballX = ballX2 - toPos.X;
                    ballY = ballY2 - toPos.Y;
                    player.xPos = ballX - player.ballSize;
                    player.yPos = ballY - player.ballSize;
                    b.health = b.health - 40;

                }
            }
        }

        //updates the text elements, or info graphics in the game
        private void UpdateInfo()
        {
            double ballX = player.xPos + player.ballSize;
            double ballY = player.yPos + player.ballSize;
            for (int i = 0; i < Information.Count(); i ++)
            {

                InfoText text = Information[i];
                //get the distance of the ball to the text position, but don't sqrt because that causes a lot of processing
                double dis = ((ballX - text.xPos) * (ballX - text.xPos) + (ballY - text.yPos) * (ballY - text.yPos));
                if(dis < text.dis * text.dis)
                {
                    //if ball is close enough it will sqrt and then calculate the transition state, consisting of opacity and position and rotate
                    dis = Math.Sqrt(dis);
                    //opacity
                    text.textInfo.Opacity = 1 - (dis / text.dis);
                    //transition position with rotation
                    Vector moveFrom = new Vector(text.xPos - text.slideFromX, text.yPos - text.slideFromY);
                    moveFrom.X = moveFrom.X * (1 - dis / text.dis);
                    moveFrom.Y = moveFrom.Y * (1 - dis / text.dis);
                    Vector p1 = new Vector((text.slideFromX + moveFrom.X), (text.slideFromY + moveFrom.Y));
                    p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);

                    double x1 = ((p1.X - (text.xSize / 2)) * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y1 = ((p1.Y - (text.ySize / 2)) * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                    Canvas.SetLeft(text.textInfo, x1);
                    Canvas.SetTop(text.textInfo, y1);
                }
                else
                {
                    text.textInfo.Opacity = 0;
                }
            }
        }

        //update the moving platforms in the game
        private void UpdatePlatforms()
        {
            for (int i = 0; i < movingPlatforms.Count; i++)
            {
                Platform p = movingPlatforms[i];
                if (p.wait < 1)
                {
                    if (p.movingTo1)
                    {
                        double dis = Distance(p.position.X, p.position.Y, p.movePoint1X, p.movePoint1Y);
                        if (dis < p.lastDis)
                        {
                            p.lastDis = (float)dis;
                        }
                        else
                        {
                            p.movingTo1 = true;
                            Vector newVel = new Vector(p.position.X - p.movePoint1X, p.position.Y - p.movePoint1Y);
                            newVel.Normalize();
                            newVel.X = newVel.X * -p.moveSpeed;
                            newVel.Y = newVel.Y * -p.moveSpeed;
                            p.vel = newVel;
                            p.wait = p.waitTime;
                        }
                        if (dis < 10)
                        {
                            p.lastDis = 10000;
                            p.movingTo1 = false;
                            Vector newVel = new Vector(p.position.X - p.movePoint2X, p.position.Y - p.movePoint2Y);
                            newVel.Normalize();
                            newVel.X = newVel.X * -p.moveSpeed;
                            newVel.Y = newVel.Y * -p.moveSpeed;
                            p.vel = newVel;
                            p.wait = p.waitTime;
                        }
                    }
                    else
                    {
                        double dis = Distance(p.position.X, p.position.Y, p.movePoint2X, p.movePoint2Y);
                        if (dis < p.lastDis)
                        {
                            p.lastDis = (float)dis;
                        }
                        else
                        {
                            p.movingTo1 = false;
                            Vector newVel = new Vector(p.position.X - p.movePoint2X, p.position.Y - p.movePoint2Y);
                            newVel.Normalize();
                            newVel.X = newVel.X * -p.moveSpeed;
                            newVel.Y = newVel.Y * -p.moveSpeed;
                            p.vel = newVel;
                            p.wait = p.waitTime;
                        }
                        if (dis < 10)
                        {
                            p.lastDis = 10000;
                            p.movingTo1 = true;
                            Vector newVel = new Vector(p.position.X - p.movePoint1X, p.position.Y - p.movePoint1Y);
                            newVel.Normalize();
                            newVel.X = newVel.X * -p.moveSpeed;
                            newVel.Y = newVel.Y * -p.moveSpeed;
                            p.vel = newVel;
                            p.wait = p.waitTime;
                        }
                    }
                    p.position.X = p.position.X + p.vel.X;
                    p.position.Y = p.position.Y + p.vel.Y;
                }
                else
                {
                    p.wait--;
                }

                CollideLine c = collides[p.index1];
                c.x2 = (float)p.position.X - (float)p.platformSize.X;
                c.y2 = (float)p.position.Y - (float)p.platformSize.Y;
                c.x1 = (float)p.position.X - (float)p.platformSize.X;
                c.y1 = (float)p.position.Y + (float)p.platformSize.Y;

                CollideLine c2 = collides[p.index2];
                c2.x2 = (float)p.position.X - (float)p.platformSize.X;
                c2.y2 = (float)p.position.Y + (float)p.platformSize.Y;
                c2.x1 = (float)p.position.X + (float)p.platformSize.X;
                c2.y1 = (float)p.position.Y + (float)p.platformSize.Y;

                CollideLine c3 = collides[p.index3];
                c3.x1 = (float)p.position.X + (float)p.platformSize.X;
                c3.y1 = (float)p.position.Y + (float)p.platformSize.Y;
                c3.x2 = (float)p.position.X + (float)p.platformSize.X;
                c3.y2 = (float)p.position.Y - (float)p.platformSize.Y;

                CollideLine c4 = collides[p.index4];
                c4.x1 = (float)p.position.X + (float)p.platformSize.X;
                c4.y1 = (float)p.position.Y - (float)p.platformSize.Y;
                c4.x2 = (float)p.position.X - (float)p.platformSize.X;
                c4.y2 = (float)p.position.Y - (float)p.platformSize.Y;
            }
        }
        private Vector RotatePoint(Vector point, Vector center, double angle)
        {
            angle = toRadians(angle);
            double cx = center.X;
            double cy = center.Y;
            Vector p1 = new Vector(point.X, point.Y);
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);

            // translate point back to origin
            p1.X = p1.X - cx;
            p1.Y = p1.Y - cy;

            // rotate point
            double xnew = p1.X * c - p1.Y * s;
            double ynew = p1.X * s + p1.Y * c;

            // translate point back
            p1.X = xnew + cx;
            p1.Y = ynew + cy;
            return p1;
            
        }

        //updates the switches and plates
        private CollideLine UpdateSwitch(CollideLine c)
        {
            double ballX = player.xPos + player.ballSize;
            double ballY = player.yPos + player.ballSize;
            if (c.MoreData[1] == 0)
            {
                if (c.activatedSpecial)
                {
                    Vector p1 = new Vector(c.x2, c.y2);
                    Vector p2 = new Vector(c.x2 + 15, c.y2 - 30);
                    p1 = RotatePoint(p1, new Vector(player.xPos + player.ballSize, player.yPos + player.ballSize), camRotate);
                    p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                    double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                    double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;

                    c.lineShape.StrokeThickness = 5;
                    c.lineShape.Stroke = Brushes.DarkRed;
                    c.lineShape.X1 = x2;
                    c.lineShape.Y1 = y2;
                    c.lineShape.X2 = x1;
                    c.lineShape.Y2 = y1;
                }
                else
                {
                    Vector p1 = new Vector(c.x1, c.y1);
                    Vector p2 = new Vector(c.x1 - 15, c.y1 - 30);
                    p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);
                    p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                    double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                    double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;

                    c.lineShape.StrokeThickness = 5;
                    c.lineShape.Stroke = Brushes.Gray;
                    c.lineShape.X1 = x2;
                    c.lineShape.Y1 = y2;
                    c.lineShape.X2 = x1;
                    c.lineShape.Y2 = y1;
                }
            }
            else
            {
                if (c.activatedSpecial == false)
                {
                    Vector p1 = new Vector(c.x2, c.y2);
                    Vector p2 = new Vector(c.x2 + 15, c.y2 - 30);
                    p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);
                    p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                    double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                    double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;


                    c.lineShape.StrokeThickness = 5;
                    c.lineShape.Stroke = Brushes.Gray;
                    c.lineShape.X1 = x2;
                    c.lineShape.Y1 = y2;
                    c.lineShape.X2 = x1;
                    c.lineShape.Y2 = y1;
                }
                else
                {
                    Vector p1 = new Vector(c.x1, c.y1);
                    Vector p2 = new Vector(c.x1 - 15, c.y1 - 30);
                    p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);
                    p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                    double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                    double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;

                    c.lineShape.StrokeThickness = 5;
                    c.lineShape.Stroke = Brushes.DarkRed;
                    c.lineShape.X1 = x2;
                    c.lineShape.Y1 = y2;
                    c.lineShape.X2 = x1;
                    c.lineShape.Y2 = y1;
                }
            }
            return c;
        }
        private bool GoThroughLines(bool foundWall, bool set, bool update)
        {
            double ballX = player.xPos + player.ballSize;
            double ballY = player.yPos + player.ballSize;
            bool hit = false;
            double biggestDot = player.ballSize;
            
            for (int i = 0; i < collides.Count; i++)
            {
                CollideLine c = collides[i];
                float addedVelX = 0;
                float addedVelY = 0;
                //gravity surface
                if(c.type == 7)
                {
                    c.lineShape.Stroke = Brushes.DarkBlue;
                    c.lineShape.StrokeThickness = 5;
                    if(player.touching && player.touchingIndex == i)
                    {
                        //if we are colliding with the ball we will set the gravity to the normal of the surface
                        gravity.X = c.normal.X * c.MoreData[0];
                        gravity.Y = -c.normal.Y * c.MoreData[0];
                    }
                }

                if(c.type == 5)
                {
                    if(update == false)
                    {
                        CollideLine toPower = collides[c.index3Refrence];
                        if(toPower.type == 4)
                        {
                            if(toPower.MoreData[12] == 1)
                            {
                                if(c.activatedSpecial)
                                {
                                    c.activatedSpecial = false;
                                }
                                else
                                {
                                    c.activatedSpecial = true;
                                }
                            }
                        }
                    }
                }
                if (c.type == 4)
                {
                    if (update == false)
                    {
                        c.MoreData[11] = 0;
                    }
                    CollideLine power = collides[c.index3Refrence];
                    if (power.activatedSpecial)
                    {
                        if (c.MoreData[9] < c.MoreData[0])
                        {
                            Vector vec = new Vector(c.MoreData[1] - c.MoreData[5], c.MoreData[2] - c.MoreData[6]);
                            vec.X = vec.X / c.MoreData[0];
                            vec.Y = vec.Y / c.MoreData[0];
                            addedVelX = (float)vec.X;
                            addedVelY = (float)vec.Y;
                            if (update == false)
                            {
                                c.x1 = c.x1 - (float)vec.X;
                                c.y1 = c.y1 - (float)vec.Y;
                            }
                            if (c.MoreData[10] == 1 && update == false)
                            {
                                if (power.type != 4)
                                {
                                    power.x1 = power.x1 - (float)vec.X;
                                    power.y1 = power.y1 - (float)vec.Y;
                                    power.x2 = power.x2 - (float)vec.X;
                                    power.y2 = power.y2 - (float)vec.Y;
                                    if (power.type == 2)
                                    {
                                       power = UpdateSwitch(power);
                                    }
                                }
                                else
                                {
                                    if(power.index3Refrence != -1)
                                    {
                                        CollideLine power2 = collides[power.index3Refrence];
                                        power2.x1 = power2.x1 - (float)vec.X;
                                        power2.y1 = power2.y1 - (float)vec.Y;
                                        power2.x2 = power2.x2 - (float)vec.X;
                                        power2.y2 = power2.y2 - (float)vec.Y;
                                        if (power2.type == 2)
                                        {
                                           power2= UpdateSwitch(power2);
                                        }
                                    }
                                }
                            }




                            vec = new Vector(c.MoreData[3] - c.MoreData[7], c.MoreData[4] - c.MoreData[8]);
                            vec.X = vec.X / c.MoreData[0];
                            vec.Y = vec.Y / c.MoreData[0];
                            if (update == false)
                            {
                                c.x2 = c.x2 - (float)vec.X;
                                c.y2 = c.y2 - (float)vec.Y;
                                c.MoreData[9]++;
                            }
                            c.MoreData[12] = 0;
                            c.activatedSpecial = false;

                        }
                        else
                        {
                            c.MoreData[12] = 1;
                            c.activatedSpecial = true;
                        }
                    }
                    else
                    {
                        if (c.MoreData[9] > 0)
                        {
                            c.MoreData[12] = 0;
                            Vector vec = new Vector(c.MoreData[5] - c.MoreData[1], c.MoreData[6] - c.MoreData[2]);
                            vec.X = vec.X / c.MoreData[0];
                            vec.Y = vec.Y / c.MoreData[0];
                            if (update == false)
                            {
                                c.x1 = c.x1 - (float)vec.X;
                                c.y1 = c.y1 - (float)vec.Y;
                            }
                            if (c.MoreData[10] == 1 && update == false)
                            {
                                if (power.type != 4)
                                {
                                    power.x1 = power.x1 - (float)vec.X;
                                    power.y1 = power.y1 - (float)vec.Y;
                                    power.x2 = power.x2 - (float)vec.X;
                                    power.y2 = power.y2 - (float)vec.Y;
                                    if(power.type == 2)
                                    {
                                       power = UpdateSwitch(power);
                                    }
                                }
                                else
                                {
                                    if (power.index3Refrence != -1)
                                    {
                                        CollideLine power2 = collides[power.index3Refrence];
                                        power2.x1 = power2.x1 - (float)vec.X;
                                        power2.y1 = power2.y1 - (float)vec.Y;
                                        power2.x2 = power2.x2 - (float)vec.X;
                                        power2.y2 = power2.y2 - (float)vec.Y;
                                        if (power2.type == 2)
                                        {
                                           power2 = UpdateSwitch(power2);
                                        }
                                    }
                                }
                            }

                            addedVelX = (float)vec.X;
                            addedVelY = (float)vec.Y;

                            vec = new Vector(c.MoreData[7] - c.MoreData[3], c.MoreData[8] - c.MoreData[4]);
                            //vec.Normalize();
                            vec.X = vec.X / c.MoreData[0];
                            vec.Y = vec.Y / c.MoreData[0];
                            if (update == false)
                            {
                                c.x2 = c.x2 - (float)vec.X;
                                c.y2 = c.y2 - (float)vec.Y;
                                c.MoreData[9]--;
                            }
                            c.activatedSpecial = false;
                        }
                        else
                        {
                            c.MoreData[12] = 1;
                            c.activatedSpecial = false;
                        }
                    }
                }
                if(c.type == 6)
                {
                    c.lineShape.Stroke = Brushes.Purple;
                    c.lineShape.StrokeThickness = 5;
                    if(player.touchingIndex == i && player.touching && player.died == 0)
                    {

                        player.died = 150;
                        lockCam = false;
                       
                    }
                }
                if (c.type == 3 && update == false)
                {
                    c.lineShape.StrokeThickness = 8;
                    if (c.canStick)
                    {
                        c.lineShape.Stroke = Brushes.DarkGreen;
                    }
                    else
                    {
                        c.lineShape.Stroke = Brushes.DarkGray;
                    }
                    CollideLine power = collides[c.index3Refrence];
                    if (power.activatedSpecial)
                    {
                        if (c.activatedSpecial == false)
                        {
                            double dis = Distance(c.x1, c.y1, c.x2, c.y2);
                            if (dis > c.MoreData[2] * 2)
                            {

                                if (c.MoreData[3] == 1)
                                {
                                    Vector movePos = new Vector(c.x1 - c.x2, c.y1 - c.y2);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x1 = c.x1 - (float)movePos.X;
                                    c.y1 = c.y1 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        friction = false;
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                                else
                                {
                                    Vector movePos = new Vector(c.x2 - c.x1, c.y2 - c.y1);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x2 = c.x2 - (float)movePos.X;
                                    c.y2 = c.y2 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        friction = false;
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (c.MoreData[3] == 1)
                            {
                                double dis = Distance(c.x1, c.y1, c.MoreData[0], c.MoreData[1]);
                                if (dis > c.MoreData[2] * 2)
                                {

                                    Vector movePos = new Vector(c.x1 - c.MoreData[0], c.y1 - c.MoreData[1]);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x1 = c.x1 - (float)movePos.X;
                                    c.y1 = c.y1 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        friction = false;
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            //TODO is this needed???
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                            }
                            else
                            {
                                double dis = Distance(c.x2, c.y2, c.MoreData[0], c.MoreData[1]);
                                if (dis > c.MoreData[2] * 2)
                                {

                                    Vector movePos = new Vector(c.x2 - c.MoreData[0], c.y2 - c.MoreData[1]);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x2 = c.x2 - (float)movePos.X;
                                    c.y2 = c.y2 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        friction = false;
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            //TODO is this needed?
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                            }

                        }


                    }
                    else
                    {
                        if (c.activatedSpecial == false)
                        {
                            if (c.MoreData[3] == 1)
                            {
                                double dis = Distance(c.x1, c.y1, c.MoreData[0], c.MoreData[1]);
                                if (dis > c.MoreData[2] * 2)
                                {

                                    Vector movePos = new Vector(c.x1 - c.MoreData[0], c.y1 - c.MoreData[1]);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x1 = c.x1 - (float)movePos.X;
                                    c.y1 = c.y1 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        friction = false;
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                            }
                            else
                            {
                                double dis = Distance(c.x2, c.y2, c.MoreData[0], c.MoreData[1]);
                                if (dis > c.MoreData[2] * 2)
                                {

                                    Vector movePos = new Vector(c.x2 - c.MoreData[0], c.y2 - c.MoreData[1]);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x2 = c.x2 - (float)movePos.X;
                                    c.y2 = c.y2 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        friction = false;
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            double dis = Distance(c.x1, c.y1, c.x2, c.y2);
                            if (dis > c.MoreData[2] * 2)
                            {

                                if (c.MoreData[3] == 1)
                                {
                                    friction = false;
                                    Vector movePos = new Vector(c.x1 - c.x2, c.y1 - c.y2);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x1 = c.x1 - (float)movePos.X;
                                    c.y1 = c.y1 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                                else
                                {
                                    Vector movePos = new Vector(c.x2 - c.x1, c.y2 - c.y1);
                                    movePos.Normalize();
                                    movePos.X = movePos.X * c.MoreData[2];
                                    movePos.Y = movePos.Y * c.MoreData[2];
                                    c.x2 = c.x2 - (float)movePos.X;
                                    c.y2 = c.y2 - (float)movePos.Y;
                                    if (i == player.touchingIndex && c.canStick)
                                    {
                                        friction = false;
                                        if (c.MoreData[4] == 0)
                                        {
                                            c.MoreData[4] = 1;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        else
                                        {
                                            player.xVel = player.xVel - movePos.X;
                                            player.yVel = player.yVel - movePos.Y;
                                            player.xVel = player.xVel + movePos.X;
                                            player.yVel = player.yVel + movePos.Y;
                                        }
                                        //xPos = xPos - movePos.X;
                                        //yPos = yPos - movePos.Y;
                                    }
                                    else if (c.canStick)
                                    {
                                        c.MoreData[4] = 0;
                                    }
                                }
                            }
                        }
                    }
                    collides[i] = c;
                }
                
                if (c.type == 1 || c.type == 0 || c.type == 3 || c.type == 4 || c.type == 6 || c.type == 7)
                {

                    //Vector p1 = rotatePoint(new Vector(c.x1, c.y1), new Vector(0, 0), 0);
                    //Vector p2 = rotatePoint(new Vector(c.x2, c.y2), new Vector(0, 0), 0);
                    Vector p1 = new Vector(c.x1, c.y1);
                    Vector p2 = new Vector(c.x2, c.y2);
                    p1 = RotatePoint(p1, new Vector(ballX,ballY), camRotate);
                    p2 = RotatePoint(p2, new Vector(ballX,ballY), camRotate);

                    double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                    double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                    double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                    c.lineShape.X1 = x1;
                    c.lineShape.Y1 = y1;
                    c.lineShape.X2 = x2;
                    c.lineShape.Y2 = y2;
                }
                if (c.type == 2)
                {
                    if (c.MoreData[0] == 1)
                    {
                        c.MoreData[0] = 0;
                        if (c.activatedSpecial)
                        {
                            if (c.MoreData[1] == 0)
                            {
                                float tmpX = c.x2;
                                float tmpY = c.y2;
                                c.x2 = c.x1;
                                c.y2 = c.y1;
                                c.x1 = tmpX;
                                c.y1 = tmpY;
                                c.activatedSpecial = false;
                                Vector v1 = new Vector();

                                v1 = new Vector(c.x2 - c.x1, c.y2 - c.y1);
                                v1.Normalize();
                                double tmp = v1.X;
                                v1.X = v1.Y * -1;
                                v1.Y = tmp;
                                c.normal.X = v1.X;
                                c.normal.Y = v1.Y;
                            }
                            else
                            {
                                float tmpX = c.x2;
                                float tmpY = c.y2;
                                c.x2 = c.x1;
                                c.y2 = c.y1;
                                c.x1 = tmpX;
                                c.y1 = tmpY;
                                c.activatedSpecial = false;
                                Vector v1 = new Vector();

                                v1 = new Vector(c.x2 - c.x1, c.y2 - c.y1);
                                v1.Normalize();
                                double tmp = v1.X;
                                v1.X = v1.Y * -1;
                                v1.Y = tmp;
                                c.normal.X = v1.X;
                                c.normal.Y = v1.Y;
                            }


                        }
                        else
                        {
                            if (c.MoreData[1] == 0)
                            {
                                float tmpX = c.x2;
                                float tmpY = c.y2;
                                c.x2 = c.x1;
                                c.y2 = c.y1;
                                c.x1 = tmpX;
                                c.y1 = tmpY;
                                c.activatedSpecial = true;
                                Vector v1 = new Vector();

                                v1 = new Vector(c.x2 - c.x1, c.y2 - c.y1);
                                v1.Normalize();
                                double tmp = v1.X;
                                v1.X = v1.Y * -1;
                                v1.Y = tmp;
                                c.normal.X = v1.X;
                                c.normal.Y = v1.Y;
                            }
                            else
                            {
                                float tmpX = c.x2;
                                float tmpY = c.y2;
                                c.x2 = c.x1;
                                c.y2 = c.y1;
                                c.x1 = tmpX;
                                c.y1 = tmpY;
                                c.activatedSpecial = true;
                                Vector v1 = new Vector();

                                v1 = new Vector(c.x2 - c.x1, c.y2 - c.y1);
                                v1.Normalize();
                                double tmp = v1.X;
                                v1.X = v1.Y * -1;
                                v1.Y = tmp;
                                c.normal.X = v1.X;
                                c.normal.Y = v1.Y;
                            }


                        }
                    }
                    if (c.MoreData[1] == 0)
                    {
                        if (c.activatedSpecial)
                        {
                            Vector p1 = new Vector(c.x2, c.y2);
                            Vector p2 = new Vector(c.x2 + 15, c.y2 - 30);
                            p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);
                            p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                            double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                            double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;

                            c.lineShape.StrokeThickness = 5;
                            c.lineShape.Stroke = Brushes.DarkRed;
                            c.lineShape.X1 = x2;
                            c.lineShape.Y1 = y2;
                            c.lineShape.X2 = x1;
                            c.lineShape.Y2 = y1;
                        }
                        else
                        {
                            Vector p1 = new Vector(c.x1, c.y1);
                            Vector p2 = new Vector(c.x1 - 15, c.y1 - 30);
                            p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);
                            p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                            double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                            double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;

                            c.lineShape.StrokeThickness = 5;
                            c.lineShape.Stroke = Brushes.Gray;
                            c.lineShape.X1 = x2;
                            c.lineShape.Y1 = y2;
                            c.lineShape.X2 = x1;
                            c.lineShape.Y2 = y1;
                        }
                    }
                    else
                    {
                        if (c.activatedSpecial == false)
                        {
                            Vector p1 = new Vector(c.x2, c.y2);
                            Vector p2 = new Vector(c.x2 + 15, c.y2 - 30);
                            p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);
                            p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                            double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                            double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;


                            c.lineShape.StrokeThickness = 5;
                            c.lineShape.Stroke = Brushes.Gray;
                            c.lineShape.X1 = x2;
                            c.lineShape.Y1 = y2;
                            c.lineShape.X2 = x1;
                            c.lineShape.Y2 = y1;
                        }
                        else
                        {
                            Vector p1 = new Vector(c.x1, c.y1);
                            Vector p2 = new Vector(c.x1 - 15, c.y1 - 30);
                            p1 = RotatePoint(p1, new Vector(ballX, ballY), camRotate);
                            p2 = RotatePoint(p2, new Vector(ballX, ballY), camRotate);

                            double x1 = (p1.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y1 = (p1.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;
                            double x2 = (p2.X * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2;
                            double y2 = (p2.Y * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2;

                            c.lineShape.StrokeThickness = 5;
                            c.lineShape.Stroke = Brushes.DarkRed;
                            c.lineShape.X1 = x2;
                            c.lineShape.Y1 = y2;
                            c.lineShape.X2 = x1;
                            c.lineShape.Y2 = y1;
                        }
                    }
                    collides[i] = c;
                }
                if (c.type == 1 && update == false)
                {
                    CollideLine c2 = collides[c.index1Refrence];
                    CollideLine c3 = collides[c.index2Refrence];
                    

                    if ((player.touchingIndex == i || c.index1Refrence == player.touchingIndex || c.index2Refrence == player.touchingIndex) && player.lastTouchingTime > 1)
                    {
                       
                        if (c.activatedSpecial == false)
                        {
                            c.lineShape.Stroke = Brushes.DarkRed;
                            c2.lineShape.Stroke = Brushes.DarkRed;
                            c3.lineShape.Stroke = Brushes.DarkRed;
                            c.activatedSpecial = true;
                            if(c.index3Refrence == 1)
                            {
                                c.index3Refrence = 2;
                            }
                            c.x1 = c.x1 + (float)c.normal.X * 5;
                            c.y1 = c.y1 + (float)c.normal.Y * 5;
                            c.x2 = c.x2 + (float)c.normal.X * 5;
                            c.y2 = c.y2 + (float)c.normal.Y * 5;
                            c2.x2 = c.x1;
                            c2.y2 = c.y1;
                            c3.x1 = c.x2;
                            c3.y1 = c.y2;
                        }
                    }
                    else if(c.index3Refrence != 2)
                    {
                        if (c.activatedSpecial)
                        {
                            c.activatedSpecial = false;
                            c.x1 = c.x1 - (float)c.normal.X * 5;
                            c.y1 = c.y1 - (float)c.normal.Y * 5;
                            c.x2 = c.x2 - (float)c.normal.X * 5;
                            c.y2 = c.y2 - (float)c.normal.Y * 5;
                            c2.x2 = c.x1;
                            c2.y2 = c.y1;
                            c3.x1 = c.x2;
                            c3.y1 = c.y2;
                            c.lineShape.Stroke = Brushes.Gray;
                            c2.lineShape.Stroke = Brushes.Gray;
                            c3.lineShape.Stroke = Brushes.Gray;
                        }
                    }
                    collides[i] = c;
                    collides[c.index1Refrence] = c2;
                    collides[c.index2Refrence] = c3;

                }
                //check the collisions of the ball and the lines
                addedVelX = addedVelX * 1;
                addedVelY = addedVelY * 1;
                if(c.type == 2)
                {
                    addedVelY = (float)player.yVel;
                }
                double dotVel = (c.normal.X * (player.xVel + addedVelX) + c.normal.Y * ((player.yVel * -1) + addedVelY));
                Vector v = new Vector(c.x1 - ballX, c.y1 - ballY);
                double dot2 = (v.X * c.normal.X + v.Y * c.normal.Y);
                if ((dotVel > 0))// || (c.isMovingPlatform  && touchingIndex != i))
                {
                    //get a vector for the line we are going to test collision with
                    Vector v1 = new Vector(c.x1 - c.x2, c.y1 - c.y2);
                    Vector vel = new Vector(c.normal.X, c.normal.Y);

                    //project the ball position onto the line vector, then get a collision point through this
                    double perp1 = PerpVec(v1, vel);
                    double perp2 = PerpVec(v, vel);

                    double length = (perp2 / perp1) * v1.Length;
                    double v1Length = v1.Length;
                    v1.Normalize();
                    v1.X = v1.X * length;
                    v1.Y = v1.Y * length;
                    Vector collisionPoint = new Vector(c.x1 - v1.X, c.y1 - v1.Y);
                    bool canCollide = true;
                    int maxLength = 10;
                    if(c.type == 3)
                    {
                        maxLength = 0;
                    }
                    //if the distance the projection has is less than the actaul line then we won't collide with it
                    if (length < -maxLength)
                    {
                        collisionPoint.X = c.x1;
                        collisionPoint.Y = c.y1;
                        canCollide = false;
                    }
                    //if the distance of the projection is greater than the line we won't collide with it
                    if (length > v1Length + maxLength)
                    {
                        collisionPoint.X = c.x2;
                        collisionPoint.Y = c.y2;
                        canCollide = false;
                    }
                    //if we get a garbage number we will just set it to the ball position odds are its due to the length being tiny
                    if (double.IsInfinity(collisionPoint.Y))
                    {
                        collisionPoint.Y = ballY;
                    }
                    if (double.IsInfinity(collisionPoint.X))
                    {
                        collisionPoint.X = ballX;
                    }
                    v1 = new Vector(c.x1 - c.x2, c.y1 - c.y2);
                    v = new Vector(collisionPoint.X - ballX, collisionPoint.Y - ballY);
                    vel = new Vector(player.xVel, player.yVel * -1);
                    double dot = (v.X * c.normal.X + v.Y * c.normal.Y);
                    //
                    if (dot < biggestDot && canCollide && c.lastDot[0] > biggestDot - 10)
                    {
                        if (set)
                        {
                            c.lastDot[0] = 0;
                        }

                        //  dot = (vel.X * c.normal.X + (vel.Y) * c.normal.Y);
                        // vel.X = vel.X - (2 * (dot * c.normal.X));
                        //vel.Y = vel.Y - (2 * (dot * c.normal.Y));
                        //vel.Y = vel.Y * -1;
                        //vel.X = vel.X * 0.85;

                        if (0 == 0)
                        {
                            player.touching = true;
                            player.lastTouchingTime = player.maxTouchingCountDown;
                        vel = new Vector(player.xVel, player.yVel);

                        Vector destinationPoint = new Vector(ballX + vel.X, ballY - vel.Y);
                        Vector newBasePoint = new Vector(ballX, ballY);
                        //Vector vec = new Vector(Math.Abs(c.normal.X) * vel.X, Math.Abs(c.normal.Y) * vel.Y);
                        //vec.X = vec.X / c.MoreData[0];
                        //vec.Y = vec.Y / c.MoreData[0];
                        Vector Origin = new Vector(collisionPoint.X, collisionPoint.Y);
                        Vector slideNormal = new Vector((newBasePoint.X - collisionPoint.X), (newBasePoint.Y - collisionPoint.Y));
                        slideNormal.Normalize();
                            
                        double equation3 = -(slideNormal.X * Origin.X + slideNormal.Y * Origin.Y);
                        Vector point = new Vector(destinationPoint.X, destinationPoint.Y);
                        double signedDistance = (point.X * slideNormal.X + point.Y * slideNormal.Y) + equation3;
                        Vector newDestinationPoint = new Vector(destinationPoint.X - (signedDistance * slideNormal.X), destinationPoint.Y - (signedDistance * slideNormal.Y));
                        vel.X = (newDestinationPoint.X - collisionPoint.X);
                        vel.Y = -(newDestinationPoint.Y - collisionPoint.Y);
                        //double normX = Math.Abs(Math.Round(c.normal.Y));
                        /**
                            vel.X = (vel.X * normX) + (normX * c.normal.X * -0.1);
                            vel.Y = vel.Y * Math.Abs((c.normal.X));
                            vel.Y = vel.Y * -1;
*/
                        if (foundWall == false)
                            {
                                player.touchingNormal.X = c.normal.X;
                                player.touchingNormal.Y = c.normal.Y;
                                foundWall = true;
                            }
                            if ((player.stickingOn == 2 || keyLeft) && c.normal.X < 0 && Math.Abs(c.normal.X) > Math.Abs(c.normal.Y))
                            {
                                player.touchingNormal.X = c.normal.X;
                                player.touchingNormal.Y = c.normal.Y;
                                foundWall = true;
                            }
                            if ((player.stickingOn == 1 || keyRight) && c.normal.X > 0 && Math.Abs(c.normal.X) > Math.Abs(c.normal.Y))
                            {
                                player.touchingNormal.X = c.normal.X;
                                player.touchingNormal.Y = c.normal.Y;
                                foundWall = true;
                            }
                            if ((player.stickingOn == 3 || keyUp) && c.normal.Y < 0 && Math.Abs(c.normal.X) < Math.Abs(c.normal.Y))
                            {
                                player.touchingNormal.X = c.normal.X;
                                player.touchingNormal.Y = c.normal.Y;
                                foundWall = true;
                            }
                            if (c.canStick)
                            {
                                canStick = true;
                            }
                            else
                            {
                                canStick = false;
                            }
                        }
                        if(c.type == 2)
                        {
                            c.MoreData[0] = 1;
                        }
                        player.touchingIndex = i;
                        //move the position of the ball back to where it should be
                        player.xPos = (collisionPoint.X - (player.ballSize) - c.normal.X * player.ballSize);
                        player.yPos = (collisionPoint.Y - (player.ballSize) - c.normal.Y * player.ballSize);
                        ballX = player.xPos + player.ballSize;
                        ballY = player.yPos + player.ballSize;
                        Canvas.SetLeft(Ball, (player.xPos + cameraX) + MyCanvas.ActualWidth / 2);
                        Canvas.SetTop(Ball, (player.yPos + cameraY) + MyCanvas.ActualHeight / 2);

                        if (c.type != 4 || c.MoreData[12] == 1)
                        {
                            player.xVel = vel.X;
                            player.yVel = vel.Y;
                            hit = true;
                        }
                        else if(c.MoreData[11] == 0)
                        {
                            CollideLine power = collides[c.index3Refrence];
                            
                            if (power.activatedSpecial)
                            {
                                if (c.MoreData[9] < c.MoreData[0])
                                {
                                    Vector vec = new Vector(c.MoreData[1] - c.MoreData[5], c.MoreData[2] - c.MoreData[6]);
                                    vec.X = vec.X / c.MoreData[0];
                                    vec.Y = vec.Y / c.MoreData[0];

                                    /**

                                    c.MoreData[11] = 1;
                                    Vector velo = new Vector(xVel - -vec.X, yVel - vec.Y);
                                    xVel = xVel - (velo.X * Math.Abs(c.normal.X));
                                    yVel = yVel - (velo.Y * Math.Abs(c.normal.Y));
                                    */
                                    Vector inputVel = new Vector(vec.X,-vec.Y);
                                    Vector checkVel = response(inputVel, new Vector(ballX,ballY), collisionPoint);
                                    checkVel.X = (inputVel.X - (checkVel.X));
                                    checkVel.Y = (inputVel.Y - (checkVel.Y));

                                    inputVel = new Vector(player.xVel, player.yVel);
                                    Vector vel2 = response(inputVel, new Vector(ballX, ballY), collisionPoint);

                                    player.xVel = (vel2.X);
                                    player.yVel = (vel2.Y);
                                    Vector velo = new Vector((player.xVel) - (checkVel.X), player.yVel - (checkVel.Y));

                                    player.xVel = (velo.X);
                                    player.yVel = (velo.Y);

                                }
                            }
                            else
                            {
                                if (c.MoreData[9] > 0)
                                {
                                    Vector vec = new Vector(c.MoreData[5] - c.MoreData[1], c.MoreData[6] - c.MoreData[2]);
                                    vec.X = vec.X / c.MoreData[0];
                                    vec.Y = vec.Y / c.MoreData[0];


                                    /**
                                    c.MoreData[11] = 1;
                                    Vector velo = new Vector(-xVel - vec.X, yVel - vec.Y);
                                    xVel = xVel + (velo.X * Math.Abs(c.normal.X));
                                    yVel = yVel - (velo.Y * Math.Abs(c.normal.Y));
                                    */
                                    Vector inputVel = new Vector(vec.X, -vec.Y);
                                    Vector checkVel = response(inputVel, new Vector(ballX, ballY), collisionPoint);
                                    checkVel.X = (inputVel.X - (checkVel.X));
                                    checkVel.Y = (inputVel.Y - (checkVel.Y));

                                    inputVel = new Vector(player.xVel, player.yVel);
                                    Vector vel2 = response(inputVel, new Vector(ballX, ballY), collisionPoint);

                                    player.xVel = (vel2.X);
                                    player.yVel = (vel2.Y);
                                    Vector velo = new Vector((player.xVel) - (checkVel.X), player.yVel - (checkVel.Y));

                                    player.xVel = (velo.X);
                                    player.yVel = (velo.Y);

                                }
                            }
                        }
                        //i = collides.Count + 1;
                    }
                    else
                    {
                        if (set)
                        {
                            c.lastDot[0] = dot2;
                        }
                    }
                }
                else
                {
                    if (set)
                    {
                        c.lastDot[0] = dot2;
                    }
                }
            }
            return hit;
        }
        
        private Vector response(Vector vel, Vector position, Vector collisionPoint)
        {
            double ballX = position.X;
            double ballY = position.Y;

            Vector destinationPoint = new Vector(ballX + vel.X, ballY - vel.Y);
            Vector newBasePoint = new Vector(ballX, ballY);
            //Vector vec = new Vector(Math.Abs(c.normal.X) * vel.X, Math.Abs(c.normal.Y) * vel.Y);
            //vec.X = vec.X / c.MoreData[0];
            //vec.Y = vec.Y / c.MoreData[0];
            Vector Origin = new Vector(collisionPoint.X, collisionPoint.Y);
            Vector slideNormal = new Vector((newBasePoint.X - collisionPoint.X), (newBasePoint.Y - collisionPoint.Y));
            slideNormal.Normalize();

            double equation3 = -(slideNormal.X * Origin.X + slideNormal.Y * Origin.Y);
            Vector point = new Vector(destinationPoint.X, destinationPoint.Y);
            double signedDistance = (point.X * slideNormal.X + point.Y * slideNormal.Y) + equation3;
            Vector newDestinationPoint = new Vector(destinationPoint.X - (signedDistance * slideNormal.X), destinationPoint.Y - (signedDistance * slideNormal.Y));
            vel.X = (newDestinationPoint.X - collisionPoint.X);
            vel.Y = -(newDestinationPoint.Y - collisionPoint.Y);
            return vel;
        }
        private void UpdateParticles()
        {
            for (int i = 0; i < particles.Count(); i++)
            {
                Particle p = particles[i];
                p.x = p.x + p.xVel;
                p.y = p.y + p.yVel;
                p.xVel = p.xVel + (float)gravity.X;
                p.yVel = p.yVel - (float)gravity.Y;
                p.size = p.size + p.sizeChange;
                p.shape.Width = p.size;
                p.shape.Height = p.size;

                Canvas.SetLeft(p.shape, ((p.x) * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2);
                Canvas.SetTop(p.shape, ((p.y) * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2);
                particles[i] = p;
                if (p.size < 1)
                {
                    MyCanvas.Children.Remove(p.shape);
                    particles.Remove(p);
                    i--;
                }
                
            }

        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {


            DateTime now = DateTime.Now;
            int second = (now.Second - startTime.Second);
            int minute = (now.Minute - startTime.Minute);
            if (second < 0)
            {
                second = lastSecond + 1 + Math.Abs((now.Second + addedSecond) - startTime.Second);
                minute = minute - 1;
            }
            else
            {
                lastSecond = second;
            }
            if(minute < 0)
            {
                minute = lastMinute + 1 + Math.Abs((now.Minute + addedMinute) - startTime.Minute);
            }
            else
            {
                lastMinute = minute;
            }
            string secondS = "" + second;
            string minuteS = "" + minute;
            if (second < 10)
            {
                secondS = "0" + second;
            }
            if (minute < 10)
            {
                minuteS = "0" + minute;
            }
            time.Text = "Time: " + minuteS + ":" + secondS;
            Canvas.SetLeft(time, MyCanvas.ActualWidth - 200);
            double lastCamX = cameraX;
            double lastCamY = cameraY;
            Vector p1 = new Vector(-cameraX, -cameraY);
            p1 = RotatePoint(p1, new Vector(player.xPos + player.ballSize, player.yPos + player.ballSize), camRotate);
            cameraX = -p1.X;
            cameraY = -p1.Y;
            if (paused == false)
            {
                double x1 = 100;
                double x2 = MyCanvas.ActualWidth - 100;
                double stickySt = player.stickyBoost;
                double stickyMax = player.maxBoost;
                x2 = (x2 - x1) * Math.Abs(stickySt / stickyMax);
                stickyBoostShown.StrokeThickness = 10;
                stickyBoostShown.X1 = 100;
                stickyBoostShown.Y1 = MyCanvas.ActualHeight - 10;
                stickyBoostShown.X2 = x1 + x2;
                stickyBoostShown.Y2 = MyCanvas.ActualHeight - 10;
                if (player.died == 0)
                {
                    player.xPos = player.xPos + player.xVel;
                    player.yPos = player.yPos - player.yVel;
                }
                StickOnWall();
                //yPos = yPos + addedYVel;
                //xPos = xPos + addedXVel;
                
                if (player.lastTouchingTime > 0)
                {
                    player.lastTouchingTime = player.lastTouchingTime - 1;

                }
                if (player.lastTouchingTime > 10)
                {
                    player.touching = true;
                }
                else
                {
                    player.touching = false;
                }
                UpdatePlayerAndBall();
                UpdatePlatforms();
                for (int i = 0; i < bCollides.Count; i++)
                {
                    BallCollide b = bCollides[i];
                    if (b.touchingIndex == -1)
                    {
                        if (b.movingPlatformTouch != -1)
                        {
                            b.addedXVel = 0;
                            b.addedYVel = 0;

                        }
                        b.movingPlatformTouch = -1;
                    }
                    if (b.touchingIndex != -1)
                    {
                        CollideLine c = collides[b.touchingIndex];
                        if (c.isMovingPlatform)
                        {

                            Platform p = movingPlatforms[c.platformIndex];
                            if (b.movingPlatformTouch == -1)
                            {
                                b.movingPlatformTouch = c.platformIndex;

                            }
                            b.xVel = b.xVel - p.vel.X;
                            b.yVel = b.yVel - p.vel.Y;
                            b.xVel = b.xVel + p.vel.X;
                            b.yVel = b.yVel + p.vel.Y;
                            if (p.wait < 1)
                            {
                                b.addedXVel = p.vel.X;
                                b.addedYVel = p.vel.Y;
                                //xPos = xPos + p.vel.X;
                                //yPos = yPos + p.vel.Y;
                            }
                            else
                            {
                                if (b.addedXVel != 0 || b.addedYVel != 0)
                                {
                                    b.xVel = b.xVel + b.addedXVel;
                                    b.yVel = b.yVel + b.addedYVel;
                                    b.addedXVel = 0;
                                    b.addedYVel = 0;
                                }
                            }


                        }
                        else
                        {
                            if (b.movingPlatformTouch != -1)
                            {
                                b.addedXVel = 0;
                                b.addedYVel = 0;

                            }
                            b.movingPlatformTouch = -1;
                        }
                    }




                    b.xPos = b.xPos + b.xVel;
                    b.yPos = b.yPos + b.yVel;
                    b.xVel = b.xVel + b.gravityX;
                    b.yVel = b.yVel - b.gravityY;
                    if (b.health < 1)
                    {
                        b.health = 100;
                        b.xPos = b.spawnPointX;
                        b.yPos = b.spawnPointY;
                        b.xVel = 0;
                        b.yVel = 0;
                    }
                    b.touchCount = 0;
                    b.touchingIndex = -1;
                    for (int j = 0; j < 2; j++)
                    {
                        b.CheckLineCollisions(collides, i);
                    }
                    Canvas.SetLeft(b.ball, (b.xPos * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2);
                    Canvas.SetTop(b.ball, (b.yPos * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2);
                }

                UpdateParticles();
                UpdateInfo();
                friction = true;
                bool keepGoing = GoThroughLines(false, true, false);
                if (keepGoing == false)
                {

                }
                else
                {
                    GoThroughLines(true, true, true);
                    GoThroughLines(true, true, true);
                    Canvas.SetLeft(Ball, (player.xPos - cameraX) + MyCanvas.ActualWidth / 2);
                    Canvas.SetTop(Ball, (player.yPos - cameraY) + MyCanvas.ActualHeight / 2);
                    //GoThroughLines(true, true, true);
                }


                Canvas.SetLeft(Ball, (player.xPos * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2);
                Canvas.SetTop(Ball, (player.yPos * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2);
                cameraX = lastCamX;
                cameraY = lastCamY;

                Ball.Width = (player.ballSize * 2) * zoom;
                Ball.Height = (player.ballSize * 2) * zoom;

                double dis = Distance(-player.xPos, -player.yPos, cameraX, cameraY);
                if (dis > 2 && lockCam)
                {
                    Vector moveCam = new Vector(cameraX - (-player.xPos), cameraY - (-player.yPos));
                    moveCam.Normalize();
                    moveCam.X = moveCam.X * ((dis / 10));
                    moveCam.Y = moveCam.Y * ((dis / 10));

                    cameraX = cameraX - moveCam.X;
                    cameraY = cameraY - moveCam.Y;
                }
                if (player.died > 65)
                {
                    //Canvas.SetLeft(Ball, ((xPos - (Ball.Width / zoom / 2)) * zoom + (cameraX * zoom)) + MyCanvas.ActualWidth / 2);
                    //Canvas.SetTop(Ball, ((yPos - (Ball.Width / zoom / 2)) * zoom + (cameraY * zoom)) + MyCanvas.ActualHeight / 2);
                }

                if (player.died > 100)
                {
                    if (Ball.Opacity > 0)
                    {
                        Ball.Opacity = Ball.Opacity - 0.05;

                    }

                    //Ball.Width = Ball.Width * 1.3;
                    //Ball.Height = Ball.Height * 1.3;
                    player.died = player.died - 1;
                    player.xVel = 0;
                    player.yVel = 0;
                    if (zoom < 5)
                    {
                        zoom = zoom * 1.006;
                    }
                    //camRotate = camRotate + 6;
                }
                else if (player.died > 65)
                {

                    if (zoom < 5)
                    {
                        zoom = zoom * 1.005;
                    }
                    Fade.Opacity = Fade.Opacity + (1.0 / 35.0);
                    Fade.Width = MyCanvas.ActualWidth;
                    Fade.Height = MyCanvas.ActualHeight;
                    Canvas.SetLeft(Fade, 0);
                    Canvas.SetTop(Fade, 0);
                    player.died = player.died - 1;
                    Ball.Width = (player.ballSize * 2) * zoom;
                    Ball.Height = (player.ballSize * 2) * zoom;
                    Ball.Opacity = 0;
                }
                else if (player.died > 30)
                {
                    player.xPos = player.lastSafeXPos;
                    player.yPos = player.lastSafeYPos;
                    cameraX = -player.xPos;
                    cameraY = -player.yPos;
                    camRotate = player.lastSafeCamRotate;
                    rotateTo = (float)player.lastSafeCamRotate;
                    player.xVel = player.lastSafeTouchingNormal.X;
                    player.yVel = -player.lastSafeTouchingNormal.Y;
                    zoom = 0.6;
                    player.touchingIndex = player.lastSafeTouchingIndex;
                    player.touchingNormal = player.lastSafeTouchingNormal;
                    gravity.X = player.lastSafeGravity.X;
                    gravity.Y = player.lastSafeGravity.Y;
                    Fade.Opacity = Fade.Opacity - (1.0 / 35.0);
                    Fade.Width = MyCanvas.ActualWidth;
                    Fade.Height = MyCanvas.ActualHeight;
                    Canvas.SetLeft(Fade, 0);
                    Canvas.SetTop(Fade, 0);
                    player.died = player.died - 1;
                }
                else if (player.died > 0)
                {
                    if (player.stickyBoost > 0)
                    {
                        player.stickyBoost = player.stickyBoost - 2;
                    }
                    else
                    {
                        player.stickyBoost = 0;
                    }
                    player.died = player.died - 1;
                    if (Ball.Opacity < 1)
                    {
                        Ball.Opacity = Ball.Opacity + (1.0 / 30.0);
                    }
                    lockCam = true;
                }
                if (player.yPos <= -2800 && level == 2)
                {
                    gravity.Y = 0;
                    if (player.maxBoost < 600)
                    {
                        player.maxBoost = player.maxBoost + 10;
                    }
                }
                else if (level == 2)
                {
                    if (player.yPos >= -2800 && player.yPos <= -2000)
                    {
                        gravity.Y = -0.1;
                        if (player.maxBoost > 300)
                        {
                            player.maxBoost = player.maxBoost - 10;
                        }
                    }
                }
                if (level == 2)
                {
                    if (player.xPos > 9000 && player.yPos < -1000)
                    {
                        if (Fade.Opacity < 1)
                        {
                            Fade.Opacity = Fade.Opacity + (1.0 / 100.0);
                            Fade.Width = MyCanvas.ActualWidth;
                            Fade.Height = MyCanvas.ActualHeight;
                            Canvas.SetLeft(Fade, 0);
                            Canvas.SetTop(Fade, 0);
                            lockCam = false;
                        }
                        else
                        {
                            NextLevel();
                        }
                    }
                }
                if (level == 1)
                {
                    CollideLine c1 = collides[finalSwitch];
                    if (c1.activatedSpecial && Fade.Opacity < 1)
                    {
                        Fade.Opacity = Fade.Opacity + (1.0 / 100.0);
                        Fade.Width = MyCanvas.ActualWidth;
                        Fade.Height = MyCanvas.ActualHeight;
                        Canvas.SetLeft(Fade, 0);
                        Canvas.SetTop(Fade, 0);
                        lockCam = false;
                    }
                    else if (Fade.Opacity >= 1 && c1.activatedSpecial)
                    {

                        now = DateTime.Now;
                        second = (now.Second - startTime.Second);
                        minute = (now.Minute - startTime.Minute);
                        if (second < 0)
                        {
                            second = lastSecond + 1 + Math.Abs((now.Second + addedSecond) - startTime.Second);
                            minute = minute - 1;
                        }
                        else
                        {
                            lastSecond = second;
                        }
                        double secondM = second;
                        double minuteM = minute;
                        double ScoreM = Score;
                        if (Score != 0)
                        {
                            Score = Score + (int)Math.Round(ScoreM / ((secondM + (minuteM * 60.0)) / 20.0));
                        }
                        else
                        {
                            Score = 0 + (int)Math.Round(60.0 / ((secondM + (minuteM * 60.0)) / 20.0));
                        }

                        ScoreDisplay.Text = "Score: " + (Score * 43);
                        paused = true;
                        pauseReason = 2;
                        PauseRect.Visibility = System.Windows.Visibility.Visible;
                        PauseRect.Opacity = 0;
                        ScoreDisplay.Visibility = System.Windows.Visibility.Visible;
                        ScoreDisplay.Opacity = 0;
                        ScoreSlot1.Visibility = System.Windows.Visibility.Visible;
                        ScoreSlot1.Opacity = 0;
                        ScoreSlot2.Visibility = System.Windows.Visibility.Visible;
                        ScoreSlot2.Opacity = 0;
                        ScoreSlot3.Visibility = System.Windows.Visibility.Visible;
                        ScoreSlot3.Opacity = 0;

                        ScoreSlotFill1.Visibility = System.Windows.Visibility.Visible;
                        ScoreSlotFill1.Opacity = 0;
                        ScoreSlotFill2.Visibility = System.Windows.Visibility.Visible;
                        ScoreSlotFill2.Opacity = 0;
                        ScoreSlotFill3.Visibility = System.Windows.Visibility.Visible;
                        ScoreSlotFill3.Opacity = 0;

                        ContinueButton.Visibility = System.Windows.Visibility.Visible;
                        ContinueButton.Opacity = 0;
                        ContinueButton.IsEnabled = false;

                        double WidthScale = MyCanvas.ActualWidth / 517;
                        double HeightScale = MyCanvas.ActualHeight / 319;
                        double slotSizeW = 200 * WidthScale;
                        double slotSizeH = 200 * HeightScale;

                        

                        ScoreSlotFill1.Width = slotSizeW;
                        ScoreSlotFill1.Height = slotSizeH;

                        ScoreSlotFill2.Width = slotSizeW;
                        ScoreSlotFill2.Height = slotSizeH;

                        ScoreSlotFill3.Width = slotSizeW;
                        ScoreSlotFill3.Height = slotSizeH;
                    }
                }
            }
            else if (pauseReason == 2)
            {
                double WidthScale = MyCanvas.ActualWidth / 517;
                double HeightScale = MyCanvas.ActualHeight / 319;
                Canvas.SetLeft(PauseRect, (30) * WidthScale);
                Canvas.SetTop(PauseRect, (30) * HeightScale);
                PauseRect.Width = (MyCanvas.ActualWidth - 60 * WidthScale);
                PauseRect.Height = (MyCanvas.ActualHeight - 60 * HeightScale);
                double slotSizeW = 64 * WidthScale;
                double slotSizeH = 64 * HeightScale;
                ScoreSlot1.Width = slotSizeW;
                ScoreSlot1.Height = slotSizeH;

                ScoreSlot2.Width = slotSizeW;
                ScoreSlot2.Height = slotSizeH;

                ScoreSlot3.Width = slotSizeW;
                ScoreSlot3.Height = slotSizeH;



                slotSizeW = slotSizeW / 2;
                slotSizeH = slotSizeH / 2;

                Canvas.SetLeft(ScoreSlot2, MyCanvas.ActualWidth / 2 - slotSizeW);
                Canvas.SetTop(ScoreSlot2, MyCanvas.ActualHeight / 2 - slotSizeH);

                Canvas.SetLeft(ScoreSlot1, MyCanvas.ActualWidth / 2 - (130 * WidthScale) - slotSizeW);
                Canvas.SetTop(ScoreSlot1, MyCanvas.ActualHeight / 2 - slotSizeH);

                Canvas.SetLeft(ScoreSlot3, MyCanvas.ActualWidth / 2 + (130 * WidthScale) - slotSizeW);
                Canvas.SetTop(ScoreSlot3, MyCanvas.ActualHeight / 2 - slotSizeH);

                Canvas.SetLeft(ScoreSlotFill2, MyCanvas.ActualWidth / 2 - (ScoreSlotFill2.Width / 2));
                Canvas.SetTop(ScoreSlotFill2, MyCanvas.ActualHeight / 2 - (ScoreSlotFill2.Height / 2));

                Canvas.SetLeft(ScoreSlotFill1, MyCanvas.ActualWidth / 2 - (130 * WidthScale) - (ScoreSlotFill1.Width / 2));
                Canvas.SetTop(ScoreSlotFill1, MyCanvas.ActualHeight / 2 - (ScoreSlotFill1.Height / 2));

                Canvas.SetLeft(ScoreSlotFill3, MyCanvas.ActualWidth / 2 + (130 * WidthScale) - (ScoreSlotFill3.Width / 2));
                Canvas.SetTop(ScoreSlotFill3, MyCanvas.ActualHeight / 2 - (ScoreSlotFill3.Height / 2));


                

                Canvas.SetLeft(ContinueButton, MyCanvas.ActualWidth - (150 * WidthScale));
                Canvas.SetTop(ContinueButton, MyCanvas.ActualHeight - (80 * HeightScale));

                double widthScore = 169 * WidthScale;

                ScoreDisplay.Width = widthScore;
                ScoreDisplay.Height = 26 * HeightScale;
                ScoreDisplay.FontSize = 11 * WidthScale;

                ContinueButton.Width = 75 * WidthScale;
                ContinueButton.Height = 20 * HeightScale;
                ContinueButton.FontSize = 9 * WidthScale;

                int topScore1 = 1;
                int topScore2 = 4;
                int topScore3 = 8;
                if (level == 2)
                {
                    topScore1 = 1;
                    topScore2 = 5;
                    topScore3 = 10;
                }
                if(Score == 0)
                {
                    Score = 2;
                }
                if (ScoreSlot1.Opacity >= 0.99)
                {
                    if (Score >= topScore1)
                    {
                        if (ScoreSlotFill1.Opacity < 1)
                        {
                            ScoreSlotFill1.Opacity = ScoreSlotFill1.Opacity + 0.01;

                        }
                        if (ScoreSlotFill1.Opacity > 0.1)
                        {
                            ScoreSlotFill1.Width = ScoreSlotFill1.Width - ((ScoreSlotFill1.Width - (slotSizeW * 2)) / 20.0);
                            ScoreSlotFill1.Height = ScoreSlotFill1.Height - ((ScoreSlotFill1.Height - (slotSizeH * 2)) / 20.0);
                        }


                        if (Score >= topScore2)
                        {
                            if (ScoreSlotFill1.Opacity >= 0.6)
                            {
                                if (ScoreSlotFill2.Opacity < 1)
                                {
                                    ScoreSlotFill2.Opacity = ScoreSlotFill2.Opacity + 0.01;
                                }

                                if (ScoreSlotFill2.Opacity > 0.1)
                                {
                                    ScoreSlotFill2.Width = ScoreSlotFill2.Width - ((ScoreSlotFill2.Width - (slotSizeW * 2)) / 20.0);
                                    ScoreSlotFill2.Height = ScoreSlotFill2.Height - ((ScoreSlotFill2.Height - (slotSizeH * 2)) / 20.0);
                                }


                                if (Score >= topScore3)
                                {
                                    if (ScoreSlotFill2.Opacity >= 0.6)
                                    {
                                        if (ScoreSlotFill3.Opacity < 1)
                                        {
                                            ScoreSlotFill3.Opacity = ScoreSlotFill3.Opacity + 0.01;
                                        }
                                        if (ScoreSlotFill3.Opacity > 0.1)
                                        {
                                            ScoreSlotFill3.Width = ScoreSlotFill3.Width - ((ScoreSlotFill3.Width - (slotSizeW * 2)) / 20.0);
                                            ScoreSlotFill3.Height = ScoreSlotFill3.Height - ((ScoreSlotFill3.Height - (slotSizeH * 2)) / 20.0);
                                        }
                                        if (ScoreSlotFill3.Opacity >= 0.98)
                                        {
                                            ContinueButton.IsEnabled = true;
                                        }
                                    }
                                }
                                else if (ScoreSlotFill2.Opacity >= 0.98)
                                {
                                    ScoreSlotFill3.Width = 0;
                                    ScoreSlotFill3.Height = 0;
                                    ContinueButton.IsEnabled = true;
                                }
                            }
                        }
                        else if (ScoreSlotFill1.Opacity >= 0.98)
                        {
                            ScoreSlotFill3.Width = 0;
                            ScoreSlotFill3.Height = 0;
                            ScoreSlotFill2.Width = 0;
                            ScoreSlotFill2.Height = 0;
                            ContinueButton.IsEnabled = true;
                        }
                    }
                    else
                    {
                        ScoreSlotFill1.Width = 0;
                        ScoreSlotFill1.Height = 0;
                        ScoreSlotFill3.Width = 0;
                        ScoreSlotFill3.Height = 0;
                        ScoreSlotFill2.Width = 0;
                        ScoreSlotFill2.Height = 0;
                        ContinueButton.IsEnabled = true;
                    }
                }


                Canvas.SetLeft(ScoreDisplay, MyCanvas.ActualWidth / 2 - widthScore / 2);
                Canvas.SetTop(ScoreDisplay, MyCanvas.ActualHeight / 2 - (100 * HeightScale));

                Fade.Width = MyCanvas.ActualWidth;
                Fade.Height = MyCanvas.ActualHeight;
                if (PauseRect.Opacity < 1)
                {
                    PauseRect.Opacity = PauseRect.Opacity + 0.01;
                }
                if (ScoreSlot1.Opacity < 1 && PauseRect.Opacity > 0.2)
                {
                    ScoreSlot1.Opacity = ScoreSlot1.Opacity + 0.01;
                    ScoreSlot2.Opacity = ScoreSlot2.Opacity + 0.01;
                    ScoreSlot3.Opacity = ScoreSlot3.Opacity + 0.01;

                    ScoreDisplay.Opacity = ScoreDisplay.Opacity + 0.01;
                    ContinueButton.Opacity = ContinueButton.Opacity + 0.01;
                }
            }
            else if (pauseReason == 3)
            {
                if (PauseRect.Opacity > 0 && ScoreSlot1.Opacity < 0.3)
                {
                    PauseRect.Opacity = PauseRect.Opacity - 0.005;
                }
                else if (PauseRect.Opacity <= 0)
                {
                    PauseRect.Visibility = System.Windows.Visibility.Hidden;
                    PauseRect.Opacity = 0;
                    level = level + 1;
                    pauseReason = 4;
                    LoadLevel(level);

                }
                if (ScoreSlot1.Opacity > 0)
                {
                    ScoreSlot1.Opacity = ScoreSlot1.Opacity - 0.006;
                    ScoreSlot2.Opacity = ScoreSlot2.Opacity - 0.006;
                    ScoreSlot3.Opacity = ScoreSlot3.Opacity - 0.006;

                    ScoreDisplay.Opacity = ScoreDisplay.Opacity - 0.006;
                    ContinueButton.Opacity = ContinueButton.Opacity - 0.006;

                    if (ScoreSlotFill1.Opacity > 0)
                    {
                        ScoreSlotFill1.Opacity = ScoreSlotFill1.Opacity - 0.006;
                    }
                    if (ScoreSlotFill2.Opacity > 0)
                    {
                        ScoreSlotFill2.Opacity = ScoreSlotFill2.Opacity - 0.006;
                    }
                    if (ScoreSlotFill3.Opacity > 0)
                    {
                        ScoreSlotFill3.Opacity = ScoreSlotFill3.Opacity - 0.006;
                    }
                }
                else
                {

                    ScoreDisplay.Visibility = System.Windows.Visibility.Hidden;
                    ScoreDisplay.Opacity = 0;
                    ScoreSlot1.Visibility = System.Windows.Visibility.Hidden;
                    ScoreSlot1.Opacity = 0;
                    ScoreSlot2.Visibility = System.Windows.Visibility.Hidden;
                    ScoreSlot2.Opacity = 0;
                    ScoreSlot3.Visibility = System.Windows.Visibility.Hidden;
                    ScoreSlot3.Opacity = 0;

                    ScoreSlotFill1.Visibility = System.Windows.Visibility.Hidden;
                    ScoreSlotFill1.Opacity = 0;
                    ScoreSlotFill2.Visibility = System.Windows.Visibility.Hidden;
                    ScoreSlotFill2.Opacity = 0;
                    ScoreSlotFill3.Visibility = System.Windows.Visibility.Hidden;
                    ScoreSlotFill3.Opacity = 0;

                    ContinueButton.Visibility = System.Windows.Visibility.Hidden;
                    ContinueButton.Opacity = 0;
                }
            }
            else if (pauseReason == 4)
            {
                if (Fade.Opacity > 0)
                {
                    Fade.Opacity = Fade.Opacity - 0.01;
                }
                else
                {
                    paused = false;
                }
            }
            else if (pauseReason == 5)
            {
                if (PauseRect.Opacity < 1)
                {
                    PauseRect.Opacity = PauseRect.Opacity + 0.01;
                    ScoreDisplay.Visibility = System.Windows.Visibility.Visible;
                }
                if (ScoreDisplay.Opacity < 1 && PauseRect.Opacity > 0.2)
                {
                    ScoreDisplay.Visibility = System.Windows.Visibility.Visible;
                    ContinueButton.Visibility = System.Windows.Visibility.Visible;
                    
                    ContinueButton.IsEnabled = true;
                    ScoreDisplay.Opacity = ScoreDisplay.Opacity + 0.01;
                    ContinueButton.Opacity = ContinueButton.Opacity + 0.01;
                    ContinueButton.Content = "Play Again?";
                    ScoreDisplay.Text = "Game Over.";
                }
                else if (PauseRect.Opacity >= 0.2)
                {
                    pauseReason = 6;
                }

            }
            else if(pauseReason == 6)
            {
                double WidthScale = MyCanvas.ActualWidth / 517;
                double HeightScale = MyCanvas.ActualHeight / 319;
                Canvas.SetLeft(PauseRect, (30) * WidthScale);
                Canvas.SetTop(PauseRect, (30) * HeightScale);
                PauseRect.Width = (MyCanvas.ActualWidth - 60 * WidthScale);
                PauseRect.Height = (MyCanvas.ActualHeight - 60 * HeightScale);

                ContinueButton.Width = 75 * WidthScale;
                ContinueButton.Height = 20 * HeightScale;
                ContinueButton.FontSize = 9 * WidthScale;

                Canvas.SetLeft(ContinueButton, MyCanvas.ActualWidth - (150 * WidthScale));
                Canvas.SetTop(ContinueButton, MyCanvas.ActualHeight - (80 * HeightScale));

                double widthScore = 169 * WidthScale;

                ScoreDisplay.Width = widthScore;
                ScoreDisplay.Height = 26 * HeightScale;
                ScoreDisplay.FontSize = 11 * WidthScale;

                Canvas.SetLeft(ScoreDisplay, MyCanvas.ActualWidth / 2 - widthScore / 2);
                Canvas.SetTop(ScoreDisplay, MyCanvas.ActualHeight / 2 - (100 * HeightScale));

                Fade.Width = MyCanvas.ActualWidth;
                Fade.Height = MyCanvas.ActualHeight;
            }
        }


        //Key Inputs
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.W)
            {
                keyUp = true;
                    
            }
            if (e.Key == Key.Down || e.Key == Key.S)
            {
                keyDown = true;
            }
            if (e.Key == Key.Right || e.Key == Key.D)
            {
                keyRight = true;
            }
            if (e.Key == Key.Left || e.Key == Key.A)
            {
                keyLeft = true;
            }
                

        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.W)
            {
                keyUp = false;
            }
            if (e.Key == Key.Down || e.Key == Key.S)
            {
                keyDown = false;
            }
            if (e.Key == Key.Right || e.Key == Key.D)
            {
                keyRight = false;
            }
            if (e.Key == Key.Left || e.Key == Key.A)
            {
                keyLeft = false;
            }
            if(e.Key == Key.R && player.died == 0)
            {
                player.died = 150;
            }
        }


        private void NextLevel()
        {
            DateTime now = DateTime.Now;
            int second = (now.Second - startTime.Second);
            int minute = (now.Minute - startTime.Minute);
            if (second < 0)
            {
                second = lastSecond + 1 + Math.Abs((now.Second + addedSecond) - startTime.Second);
                minute = minute - 1;
            }
            else
            {
                lastSecond = second;
            }
            double secondM = second;
            double minuteM = minute;
            double ScoreM = Score;
            if (Score != 0)
            {
                Score = Score + (int)Math.Round(ScoreM / ((secondM + (minuteM * 60.0)) / 20.0));
            }
            else
            {
                Score = 0 + (int)Math.Round(60.0 / ((secondM + (minuteM * 60.0)) / 20.0));
            }

            ScoreDisplay.Text = "Score: " + (Score * 43);
            paused = true;
            pauseReason = 2;
            PauseRect.Visibility = System.Windows.Visibility.Visible;
            PauseRect.Opacity = 0;
            ScoreDisplay.Visibility = System.Windows.Visibility.Visible;
            ScoreDisplay.Opacity = 0;
            ScoreSlot1.Visibility = System.Windows.Visibility.Visible;
            ScoreSlot1.Opacity = 0;
            ScoreSlot2.Visibility = System.Windows.Visibility.Visible;
            ScoreSlot2.Opacity = 0;
            ScoreSlot3.Visibility = System.Windows.Visibility.Visible;
            ScoreSlot3.Opacity = 0;

            ScoreSlotFill1.Visibility = System.Windows.Visibility.Visible;
            ScoreSlotFill1.Opacity = 0;
            ScoreSlotFill2.Visibility = System.Windows.Visibility.Visible;
            ScoreSlotFill2.Opacity = 0;
            ScoreSlotFill3.Visibility = System.Windows.Visibility.Visible;
            ScoreSlotFill3.Opacity = 0;

            ContinueButton.Visibility = System.Windows.Visibility.Visible;
            ContinueButton.Opacity = 0;
            ContinueButton.IsEnabled = false;

            double WidthScale = MyCanvas.ActualWidth / 517;
            double HeightScale = MyCanvas.ActualHeight / 319;
            double slotSizeW = 200 * WidthScale;
            double slotSizeH = 200 * HeightScale;
            ScoreSlotFill1.Width = slotSizeW;
            ScoreSlotFill1.Height = slotSizeH;

            ScoreSlotFill2.Width = slotSizeW;
            ScoreSlotFill2.Height = slotSizeH;

            ScoreSlotFill3.Width = slotSizeW;
            ScoreSlotFill3.Height = slotSizeH;
        }
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            
            if(pauseReason == 2)
            {
                ContinueButton.IsEnabled = false;
                pauseReason = 3;
            }
            if (pauseReason == 6)
            {
                ContinueButton.IsEnabled = false;
                pauseReason = 3;
                level = 0;
            }
        }
    }
}


