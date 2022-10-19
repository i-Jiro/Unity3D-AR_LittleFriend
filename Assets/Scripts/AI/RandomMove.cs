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
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null)
            {
                return TaskStatus.Failure;
            }

            if (IsComplete())
            {
                agent.StopMoving();
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
