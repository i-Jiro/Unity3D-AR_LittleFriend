using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;
using UnityEngine;

public class FriendController : MonoBehaviour
{
    private Camera _arCamera;
    private IGameboard _gameboard;
    private BehaviorTree _behavior;

    private void Awake()
    {
        _arCamera = Camera.main;
        _behavior = GetComponent<BehaviorTree>();
    }

    private void Start()
    {
        GameboardFactory.GameboardInitialized += GameBoardCreated;
        if (_behavior != null)
        {
            _behavior.EnableBehavior();
        }
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
    
    //Distance from player horizontally only.
    public float GetDistanceToPlayer()
    {
        Vector3 playerPos = _arCamera.transform.position;
        playerPos.y = transform.position.y;
        return Vector3.Distance(playerPos, transform.position);
    }
}
