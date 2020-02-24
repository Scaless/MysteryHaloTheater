using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MysteryHaloTheater
{
    // A player's vertical up/down rotation represented with an 11 bit range
    // 0x800 faces the horizon
    // Minimum spartan look down is 0x433, maximum look up is 0xBCC
    // Minimum *actual* appears to be 0x400 and maximum of 0xBFF, which can be observed in spaceship on LNOS
    struct PlayerVerticalRotation
    {
        private int rotation;

        public PlayerVerticalRotation(int baseRotation)
        {
            rotation = baseRotation;
        }
        public PlayerVerticalRotation(BitArray bitArray)
        {
            throw new NotImplementedException();
        }

        // Returns the rotation value in the range of -1/2pi to 1/2pi
        // Positive values are looking above the horizon, negative values are looking below the horizon
        // A value of 0 is facing directly forward
        public float ToRadians()
        {
            return (float)(rotation - 0x800) / 0x7FF * MathF.PI;
        }

        // Returns the rotation value in the range of -180.0 to 180.0
        // Positive values are looking above the horizon, negative values are looking below the horizon
        // A value of 0 is facing directly forward
        public float ToDegrees()
        {
            const float RadToDegreeRatio = 180.0f / MathF.PI;
            return ToRadians() * RadToDegreeRatio;
        }
    }

    // A player's horizontal left/right rotation represented with 13 bits
    // Ranges from 0x0000 to 0x1FFF which maps to 2pi rotation 
    struct PlayerHorizontalRotation
    {
        private int rotation;

        public PlayerHorizontalRotation(int baseRotation)
        {
            rotation = baseRotation;
        }
        public PlayerHorizontalRotation(BitArray bitArray)
        {
            throw new NotImplementedException();
        }

        // Returns the rotation value in the range of 0 to 2pi
        public float ToRadians()
        {
            return (float)(rotation) / 0x1FFF * MathF.PI * 2.0f;
        }

        // Returns the rotation value in the range of 0 to 360.0
        public float ToDegrees()
        {
            const float RadToDegreeRatio = 180.0f / MathF.PI;
            return ToRadians() * RadToDegreeRatio;
        }
    }
}
