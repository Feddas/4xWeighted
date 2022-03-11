/*****
 * This is part of a simple PropertyDrawer for string variables to allow drag
 * and drop of MonoScripts in the inspector of the Unity3d editor.
 * https://answers.unity.com/questions/1462909/assign-a-variable-of-type-type-on-the-inspector-or.html
 * 
 * NOTE: This is a runtime script and MUST NOT be placed in a folder named "editor".
 *       It also requires another editor file named "MonoScriptPropertyDrawer.cs"
 * 
 * Copyright (c) 2016 Bunny83 
 *****/
using UnityEngine;
using System;

namespace B83.Unity.Attributes
{
    /// <summary>
    /// Usage:
    ///     [B83.Unity.Attributes.MonoScript()]
    ///     public string ScriptToUse;
    ///     
    ///     System.Type typeOfScript = System.Reflection.Assembly.GetExecutingAssembly().GetType(ScriptToUse);
    ///     IScriptBaseType instanceOfScript = System.Activator.CreateInstance(typeOfScript) as IScriptBaseType;
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MonoScriptAttribute : PropertyAttribute
    {
        public System.Type type;
    }
}
