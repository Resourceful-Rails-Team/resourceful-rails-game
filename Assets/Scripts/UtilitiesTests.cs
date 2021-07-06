using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    public class UtilitiesTests : MonoBehaviour
    {
        void Start()
        {
            TestReflectCardinal();    
        }

        private void TestReflectCardinal()
        {
            bool testFailed = false;
            if(Utilities.ReflectCardinal(Cardinal.N) != Cardinal.S)
                testFailed = true;
            if(Utilities.ReflectCardinal(Cardinal.NE) != Cardinal.SW)
                testFailed = true;
            if(Utilities.ReflectCardinal(Cardinal.NW) != Cardinal.SE)
                testFailed = true;
            if(Utilities.ReflectCardinal(Cardinal.S) != Cardinal.N)
                testFailed = true;
            if(Utilities.ReflectCardinal(Cardinal.SW) != Cardinal.NE)
                testFailed = true;
            if(Utilities.ReflectCardinal(Cardinal.SE) != Cardinal.NW)
                testFailed = true;

            if(testFailed)
                Debug.LogError("Tester: TestReflectCardinal Failed");
        }
    }
}