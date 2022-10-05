using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;
using UnityEngine;

public enum FriendActionState
{ 
    Idle, Following, Wondering
}

public class FriendBehavior : MonoBehaviour
{
    private Camera _arCamera;
    private GameBoardAgent _agent;
    private float _timeUntilNextAction = 0f;
    private IGameboard _gameboard;
    
    private void Awake()
    {
        _agent = GetComponent<GameBoardAgent>();
        _arCamera = Camera.main;
    }

    private void Start()
    {
        GameboardFactory.GameboardInitialized += GameBoardCreated;
    }

    private void OnDestroy()
    {
        GameboardFactory.GameboardInitialized -= GameBoardCreated;
        if (_gameboard != null)
        {
            _gameboard.GameboardDestroyed -= GameBoardDestroyed;
        }
    }

    private void GameBoardCreated(GameboardCreatedArgs args)
    {
        _gameboard = args.Gameboard;
        _gameboard.GameboardDestroyed += GameBoardDestroyed;
    }

    private void GameBoardDestroyed(IArdkEventArgs args)
    {
        _gameboard = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeUntilNextAction < 15f && _agent.State == AgentNavigationState.Idle)
        {
            _timeUntilNextAction += Time.deltaTime;
            return;
        }
        
        if (_agent.State == AgentNavigationState.HasPath) return;
        _agent.RandomMove();
        _timeUntilNextAction = 0f;
    }

    private void FollowPlayer()
    {
        if (Vector3.Distance(_arCamera.transform.position, transform.position) < 1.0f)
        { 
            _agent.StopMoving();
            return;
        }

        if (_gameboard.FindNearestFreePosition(_arCamera.transform.position, out Vector3 nearestPosition))
        {
            _agent.SetDestination(_arCamera.transform.position);
        }
    }
}
