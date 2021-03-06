﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace restep.Core.Physics.CollisionTypes
{
    /// <summary>
    /// Circular collision bounds
    /// </summary>
    class CircleCollider : Collider
    {
        /// <summary>
        /// If there is a mismatch in scale's components, this determines whether to use the smaller component or larger component
        /// </summary>
        public bool UseSmallerScale { get; set; } = false;

        private float unscaledRadius;
        public float Radius
        {
            get
            {
                if(UseSmallerScale)
                {
                    return unscaledRadius * (Owner.Scale.X > Owner.Scale.Y ? Owner.Scale.Y : Owner.Scale.X);
                }
                else
                {
                    return unscaledRadius * (Owner.Scale.X < Owner.Scale.Y ? Owner.Scale.Y : Owner.Scale.X);
                }
            }

            set
            {
                unscaledRadius = value;
            }
        }

        public override ColliderType Type { get { return ColliderType.CT_CIRCLE; } }

        public override float InertiaTensor => (Mass * Radius * Radius) / 2.0f;

        public CircleCollider(GameObject owner, float radius) : base(owner)
        {
            Radius = radius;
            BBox = new AABBCollider(owner, new Vector2(radius, radius), false);
        }

        public override bool TestOverlap(Collider other)
        {
            switch (other.Type)
            {
                case ColliderType.CT_AABB:
                    return other.TestOverlap(this);
                case ColliderType.CT_CIRCLE:
                    return testCircle_Circle((CircleCollider)other);
                case ColliderType.CT_CONVEX:
                    return other.TestOverlap(this);
                case ColliderType.CT_OBB:
                    return other.TestOverlap(this);
            }

            return false;
        }

        private bool testCircle_Circle(CircleCollider other)
        {
            Vector2 vecBetweenCenters = other.Pos - Pos;
            return vecBetweenCenters.LengthFast <= (other.Radius + Radius);
        }

        public override bool TestPoint(Vector2 point)
        {
            Vector2 vecBetweenCenters = point - Pos;
            return vecBetweenCenters.LengthFast <= Radius;
        }

        internal override bool IsMTCTHandledByThis(ColliderType otherType)
        {
            switch (otherType)
            {
                case ColliderType.CT_AABB:
                    return false;
                case ColliderType.CT_CIRCLE:
                    return true;
                case ColliderType.CT_OBB:
                    return false;
                case ColliderType.CT_CONVEX:
                    return false;
            }
            return false;
        }
    }
}
