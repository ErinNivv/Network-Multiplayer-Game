using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;

public class RotateButton : NetworkBehaviour
{
    [SerializeField] private Transform[] rotations;

    //public void StatueRotate()
    //{
    //    StatueSnap statueSnap = GetComponent<StatueSnap>();
    //    if (statueSnap.isPlaced.Value == true)
    //    {
    //        statueSnap.statue.transform.rotation = rotations[1].transform.rotation;
    //    }
    //}
}
