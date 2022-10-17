using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Core.AI;
using UnityEngine;

namespace Core.AI
{
    [TaskDescription("A conditional to check if the player is within a certain range of the AI.")]
    public class PlayerDistanceCheck : FriendConditional
    {
        public float minimumDistance = 0;
        public override TaskStatus OnUpdate()
        {
            if (FriendController.DistanceToPlayer <= minimumDistance)
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
    }
}
