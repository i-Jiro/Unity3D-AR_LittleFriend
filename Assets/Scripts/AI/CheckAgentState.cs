using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    [TaskDescription("Checks if the current nav agent state is equal to desired state.")]
    [TaskCategory("Friend Conditional")]
    public class CheckAgentState : FriendConditional
    {
        public AgentNavigationState state = AgentNavigationState.Idle;
        public override TaskStatus OnUpdate()
        {
            if (state == agent.State)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}
