using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Core.AI
{
    public abstract class FriendConditional : Conditional
    {
        protected FriendController FriendController;
        protected GameBoardAgent agent;

        public override void OnAwake()
        {
            agent = GetComponent<GameBoardAgent>();
            FriendController = GetComponent<FriendController>();
        }
    }
}
