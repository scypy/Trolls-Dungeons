using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityMovementAI;

namespace TrollsAndDungeons
{
    public class Thief : MonoBehaviour
    {
        SteeringBasics steeringBasics;
        Wander1 wander;
        public float WanderRadius = 2f;
        public float WanderOffset = 1.5f;
        public float WanderRate = 0.4f;
        Rigidbody2D rb;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            steeringBasics = GetComponent<SteeringBasics>();
            wander = GetComponent<Wander1>();
            wander.wanderOffset = this.WanderOffset;
            wander.wanderRadius = this.WanderRadius;
            wander.wanderRate = this.WanderRate;
        }

        void FixedUpdate()
        {

            Vector3 accel = wander.GetSteering();
            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }

    }
}
