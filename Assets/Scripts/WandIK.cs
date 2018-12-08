using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandIK : MonoBehaviour
{
    public GameObject positionForHand;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        animator.SetIKPosition(AvatarIKGoal.RightHand, positionForHand.GetComponent<Transform>().position);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKRotation(AvatarIKGoal.RightHand, positionForHand.GetComponent<Transform>().rotation);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
    }
}
