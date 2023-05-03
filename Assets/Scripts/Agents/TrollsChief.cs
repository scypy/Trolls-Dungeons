using NPBehave;
using System.Collections;
using System.Collections.Generic;
using UGS;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TrollsAndDungeons
{
    public class TrollsChief : MonoBehaviour
    {

        public GameObject Player;
        private Root behaviorTree;
        private Pathfinding pathfinding;
        private Rigidbody2D rb;
        float moveSpeed = 1.5f;
        private Vector2 movement;


        public void Start()
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            pathfinding = GetComponent<Pathfinding>();
            rb = GetComponent<Rigidbody2D>();
            float maxPursuitDistance = 13f;

            behaviorTree = new Root(

                new Selector(

                    //If the agent is far away from the player, will seek the player
                    new BlackboardCondition("distanceToPlayer", Operator.IS_GREATER_OR_EQUAL, maxPursuitDistance, Stops.IMMEDIATE_RESTART,

                        new Action(() => SeekPlayer())
                    ),

                    //If the agent is close to the player, will pursue the player
                    new BlackboardCondition("distanceToPlayer", Operator.IS_SMALLER, maxPursuitDistance, Stops.IMMEDIATE_RESTART,

                        new Action(() => PursuePlayer())
                    )
                )
            );
            behaviorTree.Start();
        }


        private void SeekPlayer()
        {
            //seek speed
            moveSpeed = 2f;
            List<Vector3> path = pathfinding.FindPath(transform.position, Player.transform.position);
            MoveAgent(path);
        }

        private void PursuePlayer()
        {
            //Pursuit speed
            moveSpeed = 2f;
            List<Vector3> path = pathfinding.FindPath(transform.position, Player.transform.position);
            MoveAgent(path);
        }

        private void MoveAgent(List<Vector3> path)
        {
            if (path.Count > 0)
            {
                Vector3 targetPosition = path[0];
                Vector2 direction = (targetPosition - transform.position).normalized;
                //rb.velocity = direction * moveSpeed;
                movement = direction * moveSpeed;

            }
            else
            {
                //rb.velocity = Vector2.zero;
                movement = Vector2.zero;
                //Debug.LogError("Path count is zero");
            }
        }


        void Update()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, Player.transform.position);
            behaviorTree.Blackboard["distanceToPlayer"] = distanceToPlayer;
        }

        private void FixedUpdate()
        {
            rb.velocity = movement;
        }

    }
}
