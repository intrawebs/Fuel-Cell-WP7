#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace FuelCell
{
    class GameConstants
    {
        //mis
        public static bool DrawCollisionSpheres = false;

        //camera constants
        public const float NearClip = 1.0f;
        public const float FarClip = 1000.0f;
        public const float ViewAngle = 45.0f;

        //ship constants
        public const float Velocity = 0.75f;
        public const float TurnSpeed = 0.025f;
        public const int MaxRange = 98;

        //general
        public const int MaxRangeTerrain = 98;
        public const int NumBarriers = 40;
        public const int NumFuelCells = 12;
        public const int MinDistance = 10;
        public const int MaxDistance = 90;

        //bounding sphere scaling factors, tighten things up, change to 1 to see the difference
        public const float FuelCarrierBoundingSphereFactor = .8f;
        public const float FuelCellBoundingSphereFactor = .5f;
        public const float BarrierBoundingSphereFactor = .7f;

        //Movement
        //If you set this to 1 you will be in a state of constant turning
        //the smaller this is the more sensitive turning will be
        public const float TurnAmountGive = .3f;

        //Draw Order
        public const int DrawOrderHUD = 50;
    }
}
