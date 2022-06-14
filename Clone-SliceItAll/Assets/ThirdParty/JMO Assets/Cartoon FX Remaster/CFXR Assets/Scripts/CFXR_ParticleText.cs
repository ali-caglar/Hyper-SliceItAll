//--------------------------------------------------------------------------------------------------------------------------------
// Cartoon FX
// (c) 2012-2020 Jean Moreno
//--------------------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CartoonFX
{
	[RequireComponent(typeof(ParticleSystem))]
	public class CFXR_ParticleText : MonoBehaviour
	{
#if UNITY_EDITOR
		[Header("Text")]
		public string text;
		public float size = 1f;
		public float letterSpacing = 0.44f;

		[Header("Colors")]
		public Color backgroundColor = new Color(0, 0, 0, 1);
		public Color color1 = new Color(1, 1, 1, 1);
		public Color color2 = new Color(0, 0, 1, 1);

		[Header("Delay")]
		public float delay = 0.05f;
		public bool cumulativeDelay = false;
		[Range(0f, 2f)] public float compensateLifetime = 0;

		[Header("Misc")]
		public float lifetimeMultiplier = 1f;
		[Range(-90f, 90f)] public float rotation = -5f;
		public float sortingFudgeOffset = 0.1f;
		public CFXR_ParticleTextFontAsset font;
		public KeyCode refreshTextKey = KeyCode.R;

		void OnValidate()
		{
			this.hideFlags = HideFlags.None;

			if (text == null || font == null)
			{
				return;
			}

			// parse text to only allow valid characters
			List<char> allowed = new List<char>(font.CharSequence.ToCharArray());
			allowed.Add(' ');
			var chars = text.ToUpperInvariant().ToCharArray();
			string newText = "";
			foreach (var c in chars)
			{
				if (allowed.Contains(c))
				{
					newText += c;
				}
			}
			text = newText;

			// prevent negative or 0 size
			size = Mathf.Max(0.001f, size);
		}

		public void GenerateText()
		{
			if (text == null || font == null || !font.IsValid())
				return;

			// delete all children
			int length = this.transform.childCount;
			int overflow = 0;
			while (this.transform.childCount > 0)
			{
				Object.DestroyImmediate(this.transform.GetChild(0).gameObject);
				overflow++;
				if (overflow > 1000)
				{
					// just in case...
					Debug.LogError("Overflow!");
					break;
				}
			}

			// create one particle per character
			if (!string.IsNullOrEmpty(text))
			{
				// calculate total width offset
				float totalWidth = 0f;
				for (int i = 1; i < text.Length; i++)
				{
					if (char.IsWhiteSpace(text[i]))
					{
						totalWidth += letterSpacing * size;
					}
					else
					{
						int index = font.CharSequence.IndexOf(text[i]);
						var sprite = font.CharSprites[index];
						float charWidth = sprite.rect.width + font.CharKerningOffsets[index].post + font.CharKerningOffsets[index].pre;
						totalWidth += (charWidth * 0.01f + letterSpacing) * size;
					}
				}

				float offset = totalWidth/2f;
				totalWidth = 0f;
				for (int i = 0; i < text.Length; i++)
				{
					var letter = text[i];
					if (char.IsWhiteSpace(letter))
					{
						totalWidth += letterSpacing * size;
					}
					else
					{
						int index = font.CharSequence.IndexOf(text[i]);
						var sprite = font.CharSprites[index];

						// calculate char particle size ratio
						var ratio = size * sprite.rect.width/50f;

						// calculate char position
						totalWidth += font.CharKerningOffsets[index].pre * 0.01f * size;
						var position = (totalWidth-offset)/ratio;
						float charWidth = sprite.rect.width + font.CharKerningOffsets[index].post;
						totalWidth += (charWidth * 0.01f + letterSpacing) * size;

						// create gameobject with particle system
						var letterObj = new GameObject(letter.ToString(), typeof(ParticleSystem));
						letterObj.transform.SetParent(this.transform);
						letterObj.transform.localPosition = Vector3.zero;
						letterObj.transform.localRotation = Quaternion.identity;

						var ps = letterObj.GetComponent<ParticleSystem>();
						var sourceParticle = this.GetComponent<ParticleSystem>();
						EditorUtility.CopySerialized(sourceParticle, ps);
						letterObj.name = letter.ToString();

						// set particle system parameters
						var main = ps.main;
						main.startSizeXMultiplier *= ratio;
						main.startSizeYMultiplier *= ratio;
						main.startSizeZMultiplier *= ratio;

						ps.textureSheetAnimation.SetSprite(0, sprite);

						main.startRotation = Mathf.Deg2Rad * rotation;
						main.startColor = backgroundColor;

						var cd = ps.customData;
						cd.enabled = true;
						cd.SetColor(ParticleSystemCustomData.Custom1, color1);
						cd.SetColor(ParticleSystemCustomData.Custom2, color2);

						if (cumulativeDelay)
						{
							main.startDelay = delay * i;
							main.startLifetime = Mathf.LerpUnclamped(main.startLifetime.constant, main.startLifetime.constant + (delay * (text.Length-i)), compensateLifetime);
						}
						else
						{
							main.startDelay = delay;
						}
						main.startLifetime = main.startLifetime.constant * lifetimeMultiplier;

						// particle system renderer parameters
						var psr = ps.GetComponent<ParticleSystemRenderer>();
						var sourceParticleRenderer = this.GetComponent<ParticleSystemRenderer>();
						EditorUtility.CopySerialized(sourceParticleRenderer, psr);
						psr.enabled = true;
						psr.pivot = new Vector3(psr.pivot.x + position, psr.pivot.y, psr.pivot.z);
						psr.sortingFudge += i * sortingFudgeOffset;
					}
				}
			}

			// restart particle system
			updateFrame = 0;
			if (!editorUpdateSubscribed)
			{
				editorUpdateSubscribed = true;
				EditorApplication.update += RestartParticleSystem;
			}
		}

		int updateFrame = 0;
		bool editorUpdateSubscribed = false;
		void RestartParticleSystem()
		{
			var particleSystem = this.GetComponent<ParticleSystem>();

			if (updateFrame == 0)
			{
				particleSystem.Play(true);
			}
			else if (updateFrame == 5)
			{
				particleSystem.time = 0f;
				EditorApplication.update -= RestartParticleSystem;
				editorUpdateSubscribed = false;
			}

			updateFrame++;
		}
#endif
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(CFXR_ParticleText))]
	public class ParticleTextEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var prefab = PrefabUtility.GetPrefabInstanceStatus(target);
			if (prefab != PrefabInstanceStatus.NotAPrefab)
			{
				EditorGUILayout.HelpBox("Cartoon FX Particle Text doesn't work on Prefab Instances, as it needs to destroy/create children GameObjects.\nYou can right-click on the object, and select \"Unpack Prefab Completely\" to make it an independent Game Object.",
					MessageType.Warning);
				return;
			}

			base.OnInspectorGUI();

			GUILayout.Space(8);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			bool refresh = GUILayout.Button(" Refresh Text ", GUILayout.Height(30)) || GUI.changed;
			GUILayout.EndHorizontal();

			if (((CFXR_ParticleText)target).refreshTextKey != KeyCode.None && Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == ((CFXR_ParticleText)target).refreshTextKey)
				{
					Event.current.Use();
					refresh = true;
				}
			}

			if (refresh)
			{
				Undo.RecordObject(this.target, "Generate Particle Text");
				(this.target as CFXR_ParticleText).GenerateText();
			}
		}
	}
#endif
}