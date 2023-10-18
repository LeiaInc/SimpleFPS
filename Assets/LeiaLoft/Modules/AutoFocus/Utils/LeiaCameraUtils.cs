/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using UnityEngine;

namespace LeiaLoft
{
    public static class LeiaCameraUtils
    {
        /// <summary>
        /// Returns a baseline scaling value for a LeiaCamera based on a desired 
        /// convergence distance and leia frustum near plane distance.
        /// Useful for automatic baseline calculation scripts.
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="farPlaneDistance">The distance of the desired far plane of the Leia frustum. 
        /// Ideally should be set to the distance from the camera to the furthest currently visible point in the scene.</param>
        /// <returns>A float representing a baseline scaling value that satisfies the specified 
        /// convergence distance and Leia frustum far plane distance.</returns>
        public static float GetRecommendedBaselineBasedOnFarPlane(float farPlaneDistance, float convergenceDistance)
        {
            float recommendedBaseline;

            recommendedBaseline = farPlaneDistance / Mathf.Max(convergenceDistance - farPlaneDistance, .01f);

            return recommendedBaseline;
        }

        /// <summary>
        /// Returns a baseline scaling value for a LeiaCamera based on a desired 
        /// convergence distance and leia frustum near plane distance.
        /// Useful for automatic baseline calculation scripts.
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="nearPlaneDistance">The distance of the desired near plane of the Leia frustum. 
        /// Ideally should be set to the distance from the camera to the closest currently visible point in the scene.</param>
        /// <returns>A float representing a baseline scaling value that satisfies the specified 
        /// convergence distance and Leia frustum near plane distance.</returns>
        public static float GetRecommendedBaselineBasedOnNearPlane(float nearPlaneDistance, float convergenceDistance)
        {
            float recommendedBaseline;

            recommendedBaseline = nearPlaneDistance / Mathf.Max(convergenceDistance - nearPlaneDistance, .01f);

            return recommendedBaseline;
        }
    }
}
