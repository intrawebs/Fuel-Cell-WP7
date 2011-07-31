using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FuelCell
{
    class Helpers
    {
        public static Vector3 QuaternionToEuler(Quaternion q)
        {
            Vector3 v = Vector3.Zero;

            v.X = (float)Math.Atan2
            (
                2 * q.Y * q.W - 2 * q.X * q.Z,
                   1 - 2 * Math.Pow(q.Y, 2) - 2 * Math.Pow(q.Z, 2)
            );

            v.Z = (float)Math.Asin
            (
                2 * q.X * q.Y + 2 * q.Z * q.W
            );

            v.Y = (float)Math.Atan2
            (
                2 * q.X * q.W - 2 * q.Y * q.Z,
                1 - 2 * Math.Pow(q.X, 2) - 2 * Math.Pow(q.Z, 2)
            );

            if (q.X * q.Y + q.Z * q.W == 0.5)
            {
                v.X = (float)(2 * Math.Atan2(q.X, q.W));
                v.Y = 0;
            }

            else if (q.X * q.Y + q.Z * q.W == -0.5)
            {
                v.X = (float)(-2 * Math.Atan2(q.X, q.W));
                v.Y = 0;
            }

            return v;
        }

        /// <summary>
        /// This function is used to orient the mob towards an arbitrary point (step)
        /// </summary>
        /// <param name="step">Point the mob has to face</param>
        /// <param name="thresholdAngleCos">The cosine of the threshold angle, if the cos is inferior to this value, the mob will be rotated. 0.978f typically</param>
        /// <param name="turningSpeed">The angle in radiants the mob will be rotated to</param>
        /// <returns></returns>
        /// More efficient method and the source of this at...
        /// http://forums.create.msdn.com/forums/p/2693/13503.aspx
        public static Quaternion OrientTowardsVector3(Pawn mob, Vector3 step, float thresholdAngleCos, float turningSpeed)
        {
            Vector3 mobToTargetVector = new Vector3();
            Vector3 mobOrient = new Vector3();
            int rotationSign = 0;
            float mobToTargetAngle;
            Vector3 crossProduct = new Vector3();

            //We calculate the target to mob vector.
            mobToTargetVector = Vector3.Normalize(step - mob.Position);
            //We calculate the direction vector in the X,Y reference plane
            mobOrient = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), Matrix.CreateFromQuaternion(mob.Rotation));

            //The cross product gives out the cosine of the angle.
            mobToTargetAngle = Vector3.Dot(mobToTargetVector, mobOrient);

            if (mobToTargetAngle < thresholdAngleCos /*0.978f*/) // if the cosine value is inferior to a preset value, we have to turn
            {
                crossProduct = Vector3.Cross(mobToTargetVector, mobOrient);
                //If the cross product is positive regarding the z axis then we have to turn in the anti trigonometric sense
                //If the cross product is negative regarding the z axis then we have to turn in the trigonometric sense
                rotationSign = -Math.Sign(crossProduct.Z);
                return Quaternion.Concatenate(mob.Rotation, Quaternion.CreateFromAxisAngle(new Vector3(0f, 0f, 1f), rotationSign * turningSpeed));
            }
            else
            {
                // It is not necessary to turn, so we do not
                return mob.Rotation;
            }
        }

        public static Quaternion QuaternionDifference(Quaternion from, Quaternion to)
        {
            if (from.Z == 0 && from.Y == 0 && from.Z == 0 && from.W == 0)
            {
                return to;
            }
            else
            {
                var difference = (from * Quaternion.Inverse(from)) * to;
                return from * difference;
            }
        }

        public static void ToAxisAngle(ref Quaternion q, out Vector3 axis, out float angle)
        {
            angle = (float)Math.Acos(q.W);
            float ooScale = 1.0f / (float)Math.Sin(angle);
            angle *= 2.0f;

            axis = new Vector3(-q.X * ooScale, -q.Y * ooScale, -q.Z * ooScale);
        }

        public static bool AutoRotateDone(ref Quaternion m_quatNewOrientation, ref Quaternion m_quatCurOrientation, float m_fRotationSpeed)
        {
            //See if we should perform the slerp.
            if (m_quatNewOrientation != Quaternion.Identity)
            {
                //slerp the orientation and update rotation speed
                m_quatCurOrientation = Quaternion.Slerp(m_quatCurOrientation, m_quatNewOrientation, .1f);
                m_fRotationSpeed += 0.001f;

                //If we're done rotating to the desired orientation, stop.
                if (Math.Abs(Math.Abs(m_quatCurOrientation.X) - Math.Abs(m_quatNewOrientation.X)) < 0.001f &&
                    Math.Abs(Math.Abs(m_quatCurOrientation.Y) - Math.Abs(m_quatNewOrientation.Y)) < 0.001f &&
                    Math.Abs(Math.Abs(m_quatCurOrientation.Z) - Math.Abs(m_quatNewOrientation.Z)) < 0.001f &&
                    Math.Abs(Math.Abs(m_quatCurOrientation.W) - Math.Abs(m_quatNewOrientation.W)) < 0.001f)
                {
                    //Finalize our orientation
                    m_quatCurOrientation = m_quatNewOrientation;

                    //Reset our new orientation to the identity
                    m_quatNewOrientation = Quaternion.Identity;

                    //Let the caller know we're done.
                    return true;
                }
            }
            else //Just in case...
            {
                return true;
            }

            //We're not done rotating.
            return false;
        }
        
        public static Quaternion GetRotation(Vector3 source, Vector3 dest, Vector3 up)
        {
            float dot = Vector3.Dot(source, dest);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                // vector a and b point exactly in the opposite direction, 
                // so it is a 180 degrees turn around the up-axis
                return new Quaternion(up, MathHelper.ToRadians(180.0f));
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                // vector a and b point exactly in the same direction
                // so we return the identity quaternion
                return Quaternion.Identity;
            }

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(source, dest);
            rotAxis = Vector3.Normalize(rotAxis);
            return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
        }
    }
}
