using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace StickySlimeBall
{
    class BallCollide
    {
        public Ellipse ball = new Ellipse();
        public double xPos = 0;
        public double yPos = 0;
        public double ballSize = 0;
        public double xVel = 0;
        public double yVel = 0;
        public int type = 0;
        public float gravityX = 0;
        public float gravityY = 0;
        public int touchingIndex = -1;
        public int touchCount = 0;
        public bool touching = false;
        public int lastTouchingTime = 0;
        public int maxTouchingCountDown = 10;

        public int spawnPointX = 0;
        public int spawnPointY = 0;

        public float health = 100;

        public int movingPlatformTouch = -1;
        public double addedXVel = 0;
        public double addedYVel = 0;

        public Vector touchingNormal = new Vector(0, 0);

        private double perpVec(Vector v1, Vector v2)
        {
            Vector tmp1 = new Vector(v1.X, v1.Y);
            Vector tmp2 = new Vector(-v2.Y, v2.X);
            tmp2.Normalize();
            return (tmp1.X * tmp2.X + tmp1.Y * tmp2.Y);
        }
        public bool CheckLineCollisions(List<CollideLine> collides, int index)
        {
            //TODO test if ball's velocity is toward the normal
            double ballX = xPos + ballSize;
            double ballY = yPos + ballSize;
            bool hit = false;
            double biggestDot = ballSize;
            //iterate thorugh all the lines, and check the collisions
            for (int i = 0; i < collides.Count; i++)
            {
                //c is a line
                CollideLine c = collides[i];

                //check if the velocity we have is going toward the ball, this is for optimization
                double dotVel = (c.normal.X * xVel + c.normal.Y * (yVel * -1));

                
               

                if (dotVel > 0)
                {
                    //This dot product checks if we are on collision side of the line
                    Vector v = new Vector(c.x1 - ballX, c.y1 - ballY);
                    double dot2 = (v.X * c.normal.X + v.Y * c.normal.Y);


                    Vector v1 = new Vector(c.x1 - c.x2, c.y1 - c.y2);
                    Vector vel = new Vector(c.normal.X, c.normal.Y);
                    double perp1 = perpVec(v1, vel);
                    double perp2 = perpVec(v, vel);

                    //project the ball's mid point onto a vector that has the direction of the line we are testing the collision with
                    double length = (perp2 / perp1) * v1.Length;
                    double v1Length = v1.Length;
                    v1.Normalize();
                    v1.X = v1.X * length;
                    v1.Y = v1.Y * length;
                    Vector collisionPoint = new Vector(c.x1 - v1.X, c.y1 - v1.Y);
                    bool canCollide = true;

                    //if the collision point on a plane is before the line actually starts, then set the collision point to the end point
                    if (length < -10)
                    {
                        collisionPoint.X = c.x1;
                        collisionPoint.Y = c.y1;
                       //we know that the collision should be too far if its beyond 10, due to the ball size so we disable collision with this plane
                        canCollide = false;
                    }
                    //if its after the line ends then set it to the other end point
                    if (length > v1Length + 10)
                    {
                        collisionPoint.X = c.x2;
                        collisionPoint.Y = c.y2;
                        canCollide = false;
                    }
                    if (double.IsInfinity(collisionPoint.Y))
                    {
                        collisionPoint.Y = ballY;
                    }
                    if (double.IsInfinity(collisionPoint.X))
                    {
                        collisionPoint.X = ballX;
                    }
                    v1 = new Vector(c.x1 - c.x2, c.y1 - c.y2);
                    
                    //create a vector that holds the magnitude between the ball and the collision point
                    v = new Vector(collisionPoint.X - ballX, collisionPoint.Y - ballY);
                    vel = new Vector(xVel, yVel * -1);

                    //now we check what is the closest collision point so we can calculate that first
                    
                    double dot = (v.X * c.normal.X + v.Y * c.normal.Y);
                    if (dot < biggestDot && canCollide && c.lastDot[index + 1] > biggestDot - 10)
                    {
                        c.lastDot[index + 1] = 0;
                        if (touchingIndex != i)
                        {
                            //we iterate through all the lines a couple times so touch count increases
                            touchingIndex = i;
                            touchCount++;
                        }
                        if (type == 0)
                        {
                            //if the type is 0 we will add a friction force
                            dot = (vel.X * c.normal.X + (vel.Y) * c.normal.Y);
                            vel.X = vel.X - (2 * (dot * c.normal.X));
                            vel.Y = vel.Y - (2 * (dot * c.normal.Y));
                            vel.Y = vel.Y * -0.85;
                            vel.X = vel.X * 0.85;
                        }
                        else
                        {
                            touching = true;
                            lastTouchingTime = maxTouchingCountDown;
                            double normX = Math.Abs(c.normal.Y);
                            vel.Y = vel.Y * -1;
                            vel.X = (vel.X * normX) + (normX * c.normal.X * -0.1);
                            vel.Y = vel.Y * Math.Abs((c.normal.X));

                            touchingNormal.X = c.normal.X;
                            touchingNormal.Y = c.normal.Y;
                        }
                        //update the ball position, this is its new position
                        xPos = (collisionPoint.X - (ballSize) - c.normal.X * ballSize);
                        yPos = (collisionPoint.Y - (ballSize) - c.normal.Y * ballSize);
                        ballX = xPos;
                        ballY = yPos;
                        xVel = vel.X;
                        yVel = vel.Y;
                        hit = true;
                        i = collides.Count + 1;
                    }
                    else
                    {
                        c.lastDot[index + 1] = dot2;
                    }
                }
                else
                {
                    //This dot product checks if we are on collision side of the line
                    Vector v = new Vector(c.x1 - ballX, c.y1 - ballY);
                    double dot2 = (v.X * c.normal.X + v.Y * c.normal.Y);

                    c.lastDot[index + 1] = dot2;
                }





            }

            return hit;
        }
    }
}
