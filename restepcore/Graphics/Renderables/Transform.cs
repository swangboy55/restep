﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;


namespace restep.Graphics.Renderables
{
    internal class Transform
    {
        private Matrix3 tmat;
        private Vector2 translation;
        /// <summary>
        /// The position of the resulting transformation
        /// Modifying this value invalidates the transformation cache
        /// </summary>
        public Vector2 Translation
        {
            get
            {
                return translation;
            }

            set
            {
                translation = value;
                refreshResult = true;
                CreateTransformation(translation, out tmat);
            }
        }

        private Matrix3 rmat;
        private float rotation;
        /// <summary>
        /// The rotation of the resulting transformation
        /// Modifying this value invalidates the transformation cache
        /// </summary>
        public float Rotation
        {
            get
            {
                return rotation;
            }

            set
            {
                rotation = value;
                refreshResult = true;
                //Z axis rotation is equivalent to 2-D matrix rotation
                Matrix3.CreateRotationZ(rotation, out rmat);
            }
        }

        private Matrix3 smat;
        private Vector2 scale;
        /// <summary>
        /// The scale of the resulting transformation
        /// Modifying this value invalidates the transformation cache
        /// </summary>
        public Vector2 Scale
        {
            get
            {
                return scale;
            }

            set
            {
                scale = value;
                refreshResult = true;
                Matrix3.CreateScale(scale.X, scale.Y, 1, out smat);
            }
        }

        private bool refreshResult;
        private Matrix3 cachedTransform;
        /// <summary>
        /// Get the resultant transformation
        /// </summary>
        public Matrix3 Transformation
        {
            get
            {
                //if our cached matrix is out of date
                if(refreshResult)
                {
                    updateTransformCache();
                }
                return cachedTransform;
            }
        }

        private Matrix3 scrmat;
        private Vector2 screenSpace;
        /// <summary>
        /// Defines the size of the screen
        /// </summary>
        public Vector2 ScreenSpace
        {
            get
            {
                return screenSpace;
            }

            set
            {

            }
        }

        public Transform()
        {
            tmat = Matrix3.Identity;
            rmat = Matrix3.Identity;
            smat = Matrix3.Identity;
            cachedTransform = Matrix3.Identity;
        }

        private void updateTransformCache()
        {
            //transform order: translation * (rotation * scale)
            cachedTransform = Matrix3.Mult(tmat, Matrix3.Mult(rmat, smat));
            refreshResult = false;
        }

        private void CreateScreenspace(Vector2 screenDims, out Matrix3 outTransform)
        {
            Vect
        }

        private void CreateTransformation(Vector2 transform, out Matrix3 outTransform)
        {
            /* 2-D transformation from Matrix3:
            1 0 X
            0 1 Y
            0 0 1
            */
            outTransform.Row0 = new Vector3(1, 0, transform.X);
            outTransform.Row1 = new Vector3(0, 1, transform.Y);
            outTransform.Row2 = new Vector3(0, 0, 1);
        }
    }
}