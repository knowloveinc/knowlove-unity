using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviourPun
{

    float four, three, six, one, two, five;

    float[] sideNormals = new float[6];

    
    public int number = -1;

    private void Update()
    {
        four = Vector3.Angle(transform.TransformDirection(Vector3.up), Vector3.up);
        three = Vector3.Angle(transform.TransformDirection(-Vector3.up), Vector3.up);
        six = Vector3.Angle(transform.TransformDirection(-Vector3.right), Vector3.up);
        one = Vector3.Angle(transform.TransformDirection(Vector3.right), Vector3.up);
        two = Vector3.Angle(transform.TransformDirection(Vector3.forward), Vector3.up);
        five = Vector3.Angle(transform.TransformDirection(-Vector3.forward), Vector3.up);

        sideNormals[0] = one;
        sideNormals[1] = two;
        sideNormals[2] = three;
        sideNormals[3] = four;
        sideNormals[4] = five;
        sideNormals[5] = six;


        int lowest = -1;

        for(int i = 0; i < sideNormals.Length; i++)
        {
            if ( lowest == -1 || sideNormals[i] <= sideNormals[lowest])
                lowest = i;
        }

        number = lowest + 1;
    }

    [PunRPC]
    public void ToggleActive(bool enabled)
    {
        if(photonView.IsSceneView)
        {
            gameObject.SetActive(enabled);
        }
    }
}
