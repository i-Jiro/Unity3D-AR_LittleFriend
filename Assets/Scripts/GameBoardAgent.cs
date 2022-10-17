using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;
using UnityEngine;

public enum AgentNavigationState {Paused, Idle, HasPath}
public class GameBoardAgent : MonoBehaviour
{
    [SerializeField] private float _walkingSpeed = 3.0f;
    [SerializeField] private float _jumpDistance = 1;
    [SerializeField] private int _jumpPenalty = 2;
    [SerializeField] private PathFindingBehaviour _pathFindingBehaviour = PathFindingBehaviour.InterSurfacePreferResults;

    private IGameboard _gameboard;
    private AgentConfiguration _agentConfig;
    
    private Vector3 _currentDestination;
    private int _currentWaypoint = 0;
    private Path _path = new Path(null, Path.Status.PathInvalid);
    
    private Coroutine _actorMoveCoroutine;
    private Coroutine _actorJumpCoroutine;
    
    public AgentNavigationState State { get; set; } = AgentNavigationState.Idle;
    
    // Start is called before the first frame update
    void Start()
    {
        _agentConfig = new AgentConfiguration(_jumpPenalty, _jumpDistance, _pathFindingBehaviour);
        GameboardFactory.GameboardInitialized += OnGameBoardCreated;
    }

    private void OnGameBoardCreated(GameboardCreatedArgs args)
    {
        _gameboard = args.Gameboard;
        _gameboard.GameboardUpdated += OnGameBoardUpdated;
        _gameboard.GameboardDestroyed += OnGameBoardDestroyed;
    }

    //Recalculates path waypoints if game board was updated mid-move.
    private void OnGameBoardUpdated(GameboardUpdatedArgs args)
    {
        if (State == AgentNavigationState.Idle || _path.PathStatus == Path.Status.PathInvalid) return;
        
        //Recalculate path if board was cleared or pruned.
        if (args.PruneOrClear)
        {
            SetDestination(_currentDestination);
            return;
        }

        //Recalculate a new path if any of the previous upcoming waypoints are in removed nodes in the updated game board.
        for (int i = _currentWaypoint; i < _path.Waypoints.Count; i++)
        {
            if (args.RemovedNodes.Contains(_path.Waypoints[i].Coordinates))
            {
                SetDestination(_currentDestination);
            }
        }
    }

    private void OnGameBoardDestroyed(IArdkEventArgs args)
    {
        _gameboard = null;
        _path = new Path(null, Path.Status.PathInvalid);
    }

    private void OnDestroy()
    {
        GameboardFactory.GameboardInitialized -= OnGameBoardCreated;
        if (_gameboard == null) return;
        _gameboard.GameboardUpdated -= OnGameBoardUpdated;
        _gameboard.GameboardDestroyed -= OnGameBoardDestroyed;
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case AgentNavigationState.Idle:
                ReturnToGameBoard();
                break;
            case AgentNavigationState.HasPath:
                break;
            case AgentNavigationState.Paused:
                break;
        }
    }

    public void ReturnToGameBoard()
    {
        if (_gameboard == null || _gameboard.Area == 0) return;
        if (_gameboard.IsOnGameboard(transform.position, 0.2f)) return;

        List<Waypoint> pathToGameBoard = new List<Waypoint>();
        Vector3 nearestPosition;
        _gameboard.FindNearestFreePosition(transform.position, out nearestPosition);

        _currentDestination = nearestPosition;
        _currentWaypoint = 0;
        
        //Starting waypoint on agent.
        pathToGameBoard.Add(new Waypoint(transform.position,Waypoint.MovementType.Walk,Utils.PositionToTile(transform.position, _gameboard.Settings.TileSize)));
        //Ending waypoint on nearest open gameboard tile.
        pathToGameBoard.Add(new Waypoint(nearestPosition, Waypoint.MovementType.SurfaceEntry,Utils.PositionToTile(nearestPosition,_gameboard.Settings.TileSize)));
        
        _path = new Path(pathToGameBoard, Path.Status.PathComplete);
        
        _actorJumpCoroutine = StartCoroutine(Move(transform, _path.Waypoints));
        State = AgentNavigationState.HasPath;
    }

    public void RandomMove(float range)
    {
        Vector3 randomDestination;
        if (_gameboard.FindRandomPosition(transform.position, range, out randomDestination))
        {
            SetDestination(randomDestination);
        }
    }

    public void RandomMove()
    {
        Vector3 randomDestination;
        if (_gameboard.FindRandomPosition(out randomDestination))
        {
            SetDestination(randomDestination);
        }
    }

    public void SetDestination(Vector3 destination)
    {
        StopMoving();

        if (_gameboard == null)
            return;

        _currentDestination = destination;
        _currentWaypoint = 0;

        Vector3 startOnBoard;
        _gameboard.FindNearestFreePosition(transform.position, out startOnBoard);

        bool result = _gameboard.CalculatePath(startOnBoard, destination, _agentConfig, out _path);

        if (!result)
            State = AgentNavigationState.Idle;
        else
        {
            State = AgentNavigationState.HasPath;
            _actorMoveCoroutine = StartCoroutine(Move(this.transform, _path.Waypoints));
        }
    }

    public float RemainingDistance()
    {
        if (State == AgentNavigationState.Idle)
            return 0f;
        return Vector3.Distance(transform.position, _currentDestination);
    }
    
    public void StopMoving()
    {
        if (_actorMoveCoroutine != null)
            StopCoroutine(_actorMoveCoroutine);
    }

    private IEnumerator Move(Transform actor, IList<Waypoint> path)
    {
        var startPosition = actor.position;
        var startRotation = actor.rotation;
        var interval = 0.0f;
        var index = 0;
        
        while (index < path.Count)
        {
            //Check if the following node is on a different surface. If so, jump.
            if (path[index].Type == Waypoint.MovementType.SurfaceEntry)
            {
                yield return new WaitForSeconds(0.5f);
                _actorJumpCoroutine = StartCoroutine(Jump(actor, actor.position, path[index].WorldPosition));
                
                //Wait for jump routine to finish.
                yield return _actorJumpCoroutine;
                startPosition = actor.position;
                startRotation = actor.rotation;
            }
            else
            {
                //Otherwise move along toward to the next waypoint.
                interval += Time.deltaTime * _walkingSpeed;
                actor.position = Vector3.Lerp(startPosition, path[index].WorldPosition, interval);
            }

            Vector3 lookRotationTarget = path[index].WorldPosition - transform.position;
            lookRotationTarget.y = 0f;
            lookRotationTarget = lookRotationTarget.normalized;
            
            //Rotate agent towards path target.
            if(lookRotationTarget != Vector3.zero)
                transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(lookRotationTarget), interval);

            //When we reach target destination waypoint, move on to the next waypoint.
            if (Vector3.Distance(actor.position, path[index].WorldPosition) < 0.01f)
            {
                startPosition = actor.position;
                startRotation = actor.rotation;
                interval = 0f;
                index++;
                _currentWaypoint = index;
            }

            yield return null;
        }
        
        _actorMoveCoroutine = null;
        State = AgentNavigationState.Idle;
    }

    private IEnumerator Jump(Transform actor, Vector3 from, Vector3 to, float speed = 2.0f)
    {
        var interval = 0f;
        var startRotation = actor.rotation;
        var height = Mathf.Max(0.1f, Mathf.Abs(to.y - from.y));

        while (interval < 1.0f)
        {
            interval += Time.deltaTime * speed;
            
            Vector3 lookRotation = to - from;
            lookRotation = Vector3.ProjectOnPlane(lookRotation, Vector3.up).normalized;
            
            //Rotate agent towards the 'to' position.
            if (lookRotation != Vector3.zero)
                transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(lookRotation), interval);
            
            //Move agent along jump path.
            Vector3 currentPosition = Vector3.Lerp(from, to, interval);
            //Calculate current jump height in the path.
            currentPosition.y  = (-4.0f * height * Mathf.Pow(interval,2)) + (4.0f * height * interval) +
                              Mathf.Lerp(from.y, to.y, interval);
            
            actor.position = currentPosition;
            yield return null;
        }

        actor.position = to;
    }
}
