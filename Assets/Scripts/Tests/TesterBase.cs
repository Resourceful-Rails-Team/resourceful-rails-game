using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Rails
{
    public abstract class TesterBase
    {
        // A list of method names
        private static HashSet<string> _methods = new HashSet<string>();

        /// <summary>
        /// Asserts that the statement given is true. If not, logs an error
        /// to the console window.
        /// </summary>
        /// <param name="statement">The statement tested for veracity.</param>
        protected static void Assert(bool statement)
        {
            var trace = new StackTrace();
            if(!statement)
            {
                var method = trace.GetFrame(1).GetMethod();
                _methods.Add(method.Name);
            }
        }
        
        /// <summary>
        /// Tests the given method, confirming that it contains no
        /// untrue assertations. Writes an error to the Console window
        /// if it passed or failed.
        /// </summary>
        /// <param name="method">The method to test</param>
        protected static void TestMethod(Action method)
        {
            if (method == null) return;

            var methodName = method.Method.Name;

            try
            {
                method.Invoke();
                if (!_methods.Contains(methodName))
                    UnityEngine.Debug.Log($"Test Succeeded: method {methodName}");
                else
                    UnityEngine.Debug.LogError($"Test Failed: method {methodName}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Test Failed: method {methodName}. Threw Exception {e.GetType()} - {e.Message}");
            }
        }
 
        /// <summary>
        /// Tests the given method, confirming that it throws
        /// the given Exception type.
        /// </summary>
        /// <param name="method">The method to test</param>
        /// <param name="exceptionType">The exception type expected to be thrown</param>
        protected static void TestThrowsException(Action method, Type exceptionType)
        {
            if (method == null) return;
            var methodName = method.Method.Name;

            try
            {
                method.Invoke();
                UnityEngine.Debug.LogError($"Test Failed: method {methodName} succeeded, when expected {exceptionType}");
            }
            catch(Exception e)
            {
                if(e.GetType() == exceptionType)
                    UnityEngine.Debug.Log($"Test Succeeded: method {methodName}");
                else
                    UnityEngine.Debug.LogError($"Test Failed: method {methodName}. Received {e.GetType()} when expected {exceptionType}.");
            }
        }
    }
}