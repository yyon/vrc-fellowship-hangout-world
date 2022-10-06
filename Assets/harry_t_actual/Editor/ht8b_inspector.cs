﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(quest_stuff))]
public class quest_stuff_inspector: Editor
{
	public override void OnInspectorGUI()
	{
		quest_stuff qst = (quest_stuff)target;

		if( qst.replacements == null )
			qst.replacements = new List<mat_replacement>();

		if( qst.objs == null )
			qst.objs = new List<obj_toggly>();

		for( int i = 0; i < qst.replacements.Count; i ++ )
		{
			mat_replacement replacement = qst.replacements[ i ];

			replacement.shown = EditorGUILayout.Foldout( replacement.shown, replacement.name, true, EditorStyles.foldout );

			if( replacement.shown )
			{
				EditorGUI.indentLevel ++;
				replacement.name = EditorGUILayout.DelayedTextField( replacement.name );

				if( GUILayout.Button( "delete" ) )
				{
					qst.replacements.RemoveAt( i );
					continue;
				}

				int newCount = Mathf.Max( 0, EditorGUILayout.DelayedIntField( "size", replacement.materials.Count ));
				while( newCount < replacement.materials.Count)
					replacement.materials.RemoveAt( replacement.materials.Count - 1 );
				while( newCount > replacement.materials.Count)
					replacement.materials.Add( null );

				replacement.shader_default = (Shader)EditorGUILayout.ObjectField( replacement.shader_default, typeof( Shader ), false );
				replacement.shader_quest = (Shader)EditorGUILayout.ObjectField( replacement.shader_quest, typeof( Shader ), false );
 
				for( int j = 0; j < replacement.materials.Count; j++)
				{
					replacement.materials[ j ] = (Material)EditorGUILayout.ObjectField( replacement.materials[ j ], typeof(Material), false );
				}
				EditorGUI.indentLevel --;
			}
		}

		if( GUILayout.Button( "add shader replacement" ) )
		{
			mat_replacement replacement = new mat_replacement();
			replacement.materials = new List<Material>();
			qst.replacements.Add( replacement );
		}



		for( int i = 0; i < qst.objs.Count; i ++ )
		{
			obj_toggly toggle = qst.objs[ i ];

			toggle.shown = EditorGUILayout.Foldout( toggle.shown, toggle.name, true, EditorStyles.foldout );

			if( toggle.shown )
			{
				EditorGUI.indentLevel ++;
				toggle.name = EditorGUILayout.DelayedTextField( toggle.name );

				if( GUILayout.Button( "delete" ) )
				{
					qst.objs.RemoveAt( i );
					continue;
				}

				int newCount = Mathf.Max( 0, EditorGUILayout.DelayedIntField( "size", toggle.objs.Count ));
				while( newCount < toggle.objs.Count)
					toggle.objs.RemoveAt( toggle.objs.Count - 1 );
				while( newCount > toggle.objs.Count)
					toggle.objs.Add( null );

				EditorGUILayout.BeginHorizontal();
				toggle.pc = EditorGUILayout.Toggle( "PC", toggle.pc );
				toggle.quest = EditorGUILayout.Toggle( "Quest", toggle.quest );
				EditorGUILayout.EndHorizontal();

				for( int j = 0; j < toggle.objs.Count; j++)
				{
					toggle.objs[ j ] = (GameObject)EditorGUILayout.ObjectField( toggle.objs[ j ], typeof(GameObject), true  );
				}
				EditorGUI.indentLevel --;
			}
		}

		if( GUILayout.Button( "add toggle" ) )
		{
			obj_toggly toggle = new obj_toggly();
			toggle.objs = new List<GameObject>();
			qst.objs.Add( toggle );
		}

		EditorGUILayout.BeginHorizontal();

		bool change = false;
		bool target_pc = true;

		if( GUILayout.Button( "Set PC" ) )
		{
			change = true;
		}

		if( GUILayout.Button( "Set Quest" ) )
		{
			target_pc = false;
			change = true;
		}

		if( change )
		{
			for( int i = 0; i < qst.replacements.Count; i ++ )
			{
				mat_replacement replacement = qst.replacements[ i ];

				if( replacement.shader_default == null || replacement.shader_quest == null )
					continue;

				for( int j = 0; j < replacement.materials.Count; j ++ )
				{
					if( replacement.materials[ j ] != null )
					{
						replacement.materials[ j ].shader = target_pc? replacement.shader_default: replacement.shader_quest;
					}
				}
			}

			for( int i = 0; i < qst.objs.Count; i ++ )
			{
				obj_toggly toggle = qst.objs[ i ];
				for( int j = 0; j < toggle.objs.Count; j ++ )
				{
					if( toggle.objs[ j ] != null )
					{
						toggle.objs[ j ].SetActive( toggle.pc == target_pc || toggle.quest == !target_pc );
					}
				}
			}
		}

		EditorGUILayout.EndHorizontal();


		if( GUI.changed )
      {
			serializedObject.ApplyModifiedProperties();
         EditorUtility.SetDirty( qst );
      }

	}
}
