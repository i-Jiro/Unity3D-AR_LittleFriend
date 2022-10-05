using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _friendPrefab;
    [SerializeField] private float _spawnRange = 3f;
    [SerializeField] private TextMeshProUGUI _textPrompt;
    
    private Camera _arCamera;
    private IGameboard _gameBoard;
    private GameObject _friendAgentGameObject;
    private bool _isGameBoardRunning = false;
    private bool _hasSpawnedFriend = false;
    
    public void ARSessionStarted()
    {
        
    }

    public void ARSessionStopped()
    {
        _gameBoard.Clear();
        Destroy(_friendAgentGameObject);
        _hasSpawnedFriend = false;
    }

    private void Awake()
    {
        GameboardFactory.GameboardInitialized += OnGameBoardCreated;
        _arCamera = Camera.main;
    }

    private void Start()
    {
        //Stops mobile screen from timing out from inactivity.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void OnDestroy()
    {
        GameboardFactory.GameboardInitialized -= OnGameBoardCreated;
    }

    private void OnGameBoardCreated(GameboardCreatedArgs args)
    {
        _gameBoard = args.Gameboard;
        _isGameBoardRunning = true;
        _gameBoard.GameboardDestroyed += OnGameBoardDestroyed;
    }

    private void OnGameBoardDestroyed(IArdkEventArgs args)
    {
        _gameBoard.GameboardDestroyed -= OnGameBoardDestroyed;
        _gameBoard = null;
        _isGameBoardRunning = false;
    }

    void Update()
    {
        if (!_isGameBoardRunning) return;
        if (!_hasSpawnedFriend)
        {
            SpawnFriend();
        }
    }

    private void SpawnFriend()
    {
        if (_friendAgentGameObject != null) return;
        //Find a valid space around the AR camera to place agent.
        if (_gameBoard.FindRandomPosition(_arCamera.transform.position, _spawnRange, out Vector3 randomPosition))
        {
            _friendAgentGameObject = Instantiate(_friendPrefab, randomPosition, Quaternion.identity);
            _friendAgentGameObject.transform.LookAt(_arCamera.transform.position);
            _hasSpawnedFriend = true;
            _textPrompt.text = "Spawned!";
        }
        else
        {
            _textPrompt.text = "Cannot find available space to spawn.";
        }
    }
}
