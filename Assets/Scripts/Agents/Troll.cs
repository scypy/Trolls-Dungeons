using NPBehave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrollsAndDungeons
{
	public class Troll : MonoBehaviour
	{
        public GameObject player;
        private Root behaviorTree;
        private Pathfinding pathfinding;
        private Rigidbody2D rb;
        private float moveSpeed = 1.5f;
        private PlayerBehaviour playerBehaviour;

        void Start()
		{
            player = GameObject.FindGameObjectWithTag("Player");
            pathfinding = GetComponent<Pathfinding>();
            playerBehaviour = player.GetComponent<PlayerBehaviour>();
            rb = GetComponent<Rigidbody2D>();
            float maxPursuitDistance = 20f;

            behaviorTree = new Root(

                new Selector(

                    // If the agent is far away from the player, it will move towards player
                    new BlackboardCondition("distanceToPlayer", Operator.IS_GREATER_OR_EQUAL, maxPursuitDistance, Stops.IMMEDIATE_RESTART,

                        new Action(() => SeekPlayer())
                    ),

                    // If the agent is close to the player and the player has a torch, the agent will flee
                    new BlackboardCondition("distanceToPlayer", Operator.IS_SMALLER, maxPursuitDistance, Stops.IMMEDIATE_RESTART,
                        new Condition(() => playerBehaviour.HasTorch, Stops.IMMEDIATE_RESTART,

                            new Action(() => FleePlayer())
                        )
                    ),

                    //If the agent is close to the player and the player has no torch, agent will pursue the player
                    new BlackboardCondition("distanceToPlayer", Operator.IS_SMALLER, maxPursuitDistance, Stops.IMMEDIATE_RESTART,
                        new Condition(() => !playerBehaviour.HasTorch, Stops.IMMEDIATE_RESTART,

                            new Action(() => PursuePlayer())
                        )
                    )
                )
            );

            behaviorTree.Start();
        }
        private void SeekPlayer()
        {
            moveSpeed = 3f;
            List<Vector3> path = pathfinding.FindPath(transform.position, player.transform.position);
            MoveAgent(path);
        }

        private void PursuePlayer()
        {
            moveSpeed = 3f;
            List<Vector3> path = pathfinding.FindPath(transform.position, player.transform.position);
            MoveAgent(path);
        }

        private void FleePlayer()
        {
            moveSpeed = 3f;
            Vector3 targetFleePosition = pathfinding.CalculateFleePosition(transform.position, player.transform.position, 10f);
            List<Vector3> path = pathfinding.FindPath(transform.position, targetFleePosition);
            MoveAgent(path);
        }

        private void MoveAgent(List<Vector3> path)
        {
            if (path.Count > 0)
            {
                Vector3 targetPosition = path[0];
                Vector2 direction = (targetPosition - transform.position).normalized;
                rb.velocity = direction * moveSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }


        void Update()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            behaviorTree.Blackboard["distanceToPlayer"] = distanceToPlayer;
        }


    }
}
