using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bone : MonoBehaviour
{

    public float force;

    void Start()
    {
        Rigidbody boneRigibody = GetComponent<Rigidbody>();
        boneRigibody.AddForce(Random.insideUnitSphere * force, ForceMode.Impulse);
        boneRigibody.AddTorque(Random.insideUnitSphere * force * 6, ForceMode.Impulse);

        DOTween.Sequence().SetDelay(5).OnComplete(() => Destroy(gameObject));
    }
}
