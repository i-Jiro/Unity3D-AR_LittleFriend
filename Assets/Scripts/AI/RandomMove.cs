using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;


namespace Core.AI
{
    [TaskCategory("Friend Actions")]
    [TaskDescription("Randomly moves to a random position within a range.")]
    public class RandomMove : FriendAction
    {
        public int moveRange = 1;
        public float arriveDistance = 0.2f;

        public override void OnStart()
        {
            agent.RandomMove();
            animator.SetBool("IsMoving", true);
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null)
            {
                Debug.LogError("Gameboard agent is null!");
                return TaskStatus.Failure;
            }

            if (IsComplete())
            {
                animator.SetBool("IsMoving", false);
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
        
        private bool IsComplete()
        {
            return agent.RemainingDistance() < arriveDistance;
        }
    }
}
