using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    [TaskCategory("Friend Actions")]
    [TaskDescription("Moves toward AR Camera.")]
    public class MoveToPlayer : FriendAction
    {
        private Camera _camera;
        public float arriveDistance = 0.2f;
        
        public override void OnAwake()
        {
            _camera = Camera.main;
            base.OnAwake();
        }

        public override void OnStart()
        {
            agent.SetDestination(_camera.transform.position);
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
                agent.StopMoving();
                return TaskStatus.Success;
            }
            agent.SetDestination(_camera.transform.position);
            return TaskStatus.Running;
        }
        
        private bool IsComplete()
        {
            return agent.RemainingDistance() < arriveDistance;
        }
    }
}
