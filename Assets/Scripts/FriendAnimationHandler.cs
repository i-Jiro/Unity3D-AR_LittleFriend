using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendAnimationHandler : MonoBehaviour
{
    private Animator _animator;
    private GameBoardAgent _agent;

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        if (_agent == null)
        {
            _agent = GetComponent<GameBoardAgent>();
        }

        if (_agent == null) return;
        _agent.AgentStartMove += OnStartMove;
        _agent.AgentEndMove += OnEndMove;
        _agent.AgentJumping += OnJump;
    }

    private void OnDisable()
    {
        if (_agent == null)
        {
            _agent = GetComponent<GameBoardAgent>();
        }

        if (_agent == null) return;
        _agent.AgentStartMove -= OnStartMove;
        _agent.AgentEndMove -= OnEndMove;
        _agent.AgentJumping -= OnJump;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnStartMove()
    {
        _animator.SetBool("IsMoving", true);
    }

    private void OnEndMove()
    {
        _animator.SetBool("IsMoving", false);
    }

    private void OnJump()
    {
        _animator.SetTrigger("JumpTrigger");
    }
}
