using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Core.AI
{
    public abstract class FriendAction : Action
    {
        protected GameBoardAgent agent;

        public override void OnAwake()
        {
            agent = GetComponent<GameBoardAgent>();
        }
    }
}
