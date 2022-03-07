using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StickySlimeBall
{
    class World
    {
        List<CollideLine> collides = new List<CollideLine>();
        List<BallCollide> bCollides = new List<BallCollide>();
        List<Particle> particles = new List<Particle>();
        List<Platform> movingPlatforms = new List<Platform>();
        List<InfoText> Information = new List<InfoText>();


        Vector gravity = new Vector(0, -0.1);
        bool friction = true;
        bool canStick = false;
    }
}
