using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using Com.Github.Knose1.Common.Attributes;
using Com.Github.Knose1.Common.Attributes.Abstract;

namespace Com.Github.Knose1.Common.Editor {

	[CustomEditor(typeof(BetterEditor), true), CanEditMultipleObjects()]
	public class BetterEditorEditor : UnityEditor.Editor {
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			//Script
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((BetterEditor)target), typeof(BetterEditor), false);
			GUI.enabled = true;

			SerializedProperty property = serializedObject.GetIterator();
			if (property.NextVisible(true))
			{
				Type type = target.GetType();
				do
				{
					FieldInfo field = type.GetField(property.name, BindingFlags.Instance | BindingFlags.Public);
					if (field == null) field = type.GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic);
					if (field == null) continue;

					DisabledInEditor    disabledInEditor		= (DisabledInEditor)	Attribute.GetCustomAttribute(field, typeof(DisabledInEditor));
					DisabledInRuntime   disabledInRuntime		= (DisabledInRuntime)	Attribute.GetCustomAttribute(field, typeof(DisabledInRuntime));
					Attribute[]         disabledOnList			= Attribute.GetCustomAttributes(field, typeof(DisabledOn));
					DisabledIfComponent disabledIfComponent		= (DisabledIfComponent) Attribute.GetCustomAttribute(field, typeof(DisabledIfComponent));
					
					Attribute[] disabledList = Attribute.GetCustomAttributes(field, typeof(Disabled));

					Disabled disabled = null;
					for (int i = disabledList.Length - 1; i >= 0; i--)
					{
						if (disabledList[i].GetType() == typeof(Disabled))
						{
							disabled = (Disabled)disabledList[i];
						}
					}

					bool isDisabled = false;
					bool isHide = false;

					bool updateIsDisabled(bool pIsDisabled)
					{
						isDisabled = isDisabled || pIsDisabled;
						return pIsDisabled;
					}
					void updateIsHide(bool pIsDisabled, bool pIsHide)
					{
						isHide = isHide || (pIsDisabled && pIsHide);
					}

					//*****************//
					// GET IS DISABLED //
					//*****************//
					if (disabled != null)
						updateIsHide(updateIsDisabled(true), false);

					if (disabledInEditor != null) 
					{
						updateIsHide(updateIsDisabled(!Application.isPlaying), disabledInEditor.hide);
					}

					if (disabledInRuntime != null)
					{
						updateIsHide(updateIsDisabled(Application.isPlaying), disabledInRuntime.hide);
					}

					for (int i = disabledOnList.Length - 1; i >= 0; i--) {

						if (isDisabled && isHide) break;

						DisabledOn disabledOn = (DisabledOn)disabledOnList[i];

						FieldInfo lField2 = type.GetField(disabledOn.onField, BindingFlags.Instance | BindingFlags.Public);
						if (lField2 == null) lField2 = type.GetField(disabledOn.onField, BindingFlags.Instance | BindingFlags.NonPublic);
						if (lField2 == null) throw new Exception("No field :\""+ disabledOn.onField + "\"");

						updateIsHide(updateIsDisabled(disabledOn.onCondition(lField2, target)), disabledOn.hide);
					}

					if (disabledIfComponent != null)
					{
						updateIsHide(updateIsDisabled(disabledIfComponent.Test(((BetterEditor)target).gameObject)), disabledIfComponent.hide);
					}


					//If is disabled and hidden
					if (isDisabled && isHide)
						continue;

					GUI.enabled = !isDisabled;
					EditorGUILayout.PropertyField(property);
					GUI.enabled = true;


				} while (property.NextVisible(false));
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}