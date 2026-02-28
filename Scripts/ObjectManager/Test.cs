using Assets.CreateObjects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.ObjectManager
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private Object3DAsset[] object3DAssets;
        private void Start()
        {
            foreach (Object3DAsset object3 in object3DAssets)
            {
                object3.CreateObject();
            }
        }       
    }
}
