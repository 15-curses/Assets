using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Console
{
    public class TestCommands : MonoBehaviour
    {
        [Callable("test")]
        public void TestMethod()
        {
            Debug.Log("TestMethod вызван");
        }
        [Callable("test0")]
        public void TestMethod0(string message)
        {
            Debug.Log($"TestMethod вызван: {message}");
        }
    }
}
