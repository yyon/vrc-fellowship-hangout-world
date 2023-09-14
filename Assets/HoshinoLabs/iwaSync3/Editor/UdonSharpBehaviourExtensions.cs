﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UdonSharp;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    internal static class UdonSharpBehaviourExtensions
    {
        internal static object GetPublicVariable(this UdonSharpBehaviour self, string symbolName)
        {
            return null;
        }

        internal static void SetPublicVariable<T>(this UdonSharpBehaviour self, string symbolName, T value)
        {
            var field = self.GetType().GetField(symbolName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, value);
        }
    }
}
