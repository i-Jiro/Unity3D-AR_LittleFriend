using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;
using UnityEngine;

public class FriendController : MonoBehaviour
{
    public float DistanceToPlayer { get; private set; } = 0.0f;
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

    // Update is called once per frame
    void Update()
    {
        CalculateDistanceToPlayer();
        //Debug.Log(DistanceToPlayer);
    }

    //Distance from player horizontally only.
    private void CalculateDistanceToPlayer()
    {
        Vector3 playerPos = _arCamera.transform.position;
        playerPos.y = transform.position.y;
        DistanceToPlayer = Vector3.Distance(playerPos, transform.position);
    }
}
