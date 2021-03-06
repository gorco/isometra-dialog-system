﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

namespace Isometra.Sequences {
	[CustomEditor(typeof(MultiFork))]
	public class MultiForkEditor : NodeContentEditor {
	    
	    private MultiFork multifork;
	    
	    protected override void OnEnable()
	    {
	        base.OnEnable();

	        multifork = target as MultiFork;
	        editor = CreateEditor(multifork.ForkGroup);
	    }

	    private Editor editor;
	    protected override void NodeContentInspectorGUI()
	    {
	        EditorGUILayout.HelpBox("Each fork in the list result in a different branch", MessageType.Info);
	        if(editor != null)
	        {
	            editor.OnInspectorGUI();
	        }
	    }

		
	}
}