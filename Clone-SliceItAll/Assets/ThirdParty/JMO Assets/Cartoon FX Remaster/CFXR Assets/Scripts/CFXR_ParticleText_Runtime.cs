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
	public class CFXR_ParticleText_Runtime : MonoBehaviour
	{
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

#if UNITY_EDITOR
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
#endif

		void Awake()
		{
			InitializeFirstParticle();
		}

		float baseLifetime;
		float baseScaleX;
		float baseScaleY;
		float baseScaleZ;
		Vector3 basePivot;

		public void InitializeFirstParticle()
		{
			if (this.transform.childCount == 0)
			{
				Debug.LogError("CFXR_ParticleText_Runtime requires a child with a Particle System component to act as the model for other letters.");
				return;
			}

			var particleSystem = this.transform.GetChild(0).GetComponent<ParticleSystem>();

			baseLifetime = particleSystem.main.startLifetime.constant;
			baseScaleX = particleSystem.main.startSizeXMultiplier;
			baseScaleY = particleSystem.main.startSizeYMultiplier;
			baseScaleZ = particleSystem.main.startSizeZMultiplier;
			basePivot = particleSystem.GetComponent<ParticleSystemRenderer>().pivot;
		}

		public void GenerateText(string text)
		{
			if (text == null || font == null || !font.IsValid())
			{
				return;
			}

			if (this.transform.childCount == 0)
			{
				Debug.LogError("CFXR_ParticleText_Runtime requires a child with a Particle System component to act as the model for other letters.");
				return;
			}

			// process text and calculate total width offset
			float totalWidth = 0f;
			int charCount = 0;
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsWhiteSpace(text[i]))
				{
					if (i > 0)
					{
						totalWidth += letterSpacing * size;
					}
				}
				else
				{
					charCount++;

					if (i > 0)
					{
						int index = font.CharSequence.IndexOf(text[i]);
						var sprite = font.CharSprites[index];
						float charWidth = sprite.rect.width + font.CharKerningOffsets[index].post + font.CharKerningOffsets[index].pre;
						totalWidth += (charWidth * 0.01f + letterSpacing) * size;
					}
				}
			}

#if UNITY_EDITOR
			// delete all children in editor, to make sure we refresh the particle systems based on the first one
			if (!Application.isPlaying)
			{
				int length = this.transform.childCount;
				int overflow = 0;
				while (this.transform.childCount > 1)
				{
					Object.DestroyImmediate(this.transform.GetChild(this.transform.childCount - 1).gameObject);
					overflow++;
					if (overflow > 1000)
					{
						// just in case...
						Debug.LogError("Overflow!");
						break;
					}
				}
			}
#endif

			// calculate needed instantiations
			int childCount = this.transform.childCount - 1; // first one is the particle source and always deactivated
			if (childCount < charCount)
			{
				// instantiate new letter GameObjects if needed
				GameObject model = this.transform.GetChild(0).gameObject;
				for (int i = childCount; i < charCount; i++)
				{
					var newLetter = Instantiate(model);
					newLetter.transform.SetParent(this.transform);
					newLetter.transform.localPosition = Vector3.zero;
					newLetter.transform.localRotation = Quaternion.identity;
				}
			}

			// update each letter
			float offset = totalWidth / 2f;
			totalWidth = 0f;
			int currentChild = 0;
			for (int i = 0; i < text.Length; i++)
			{
				var letter = text[i];
				if (char.IsWhiteSpace(letter))
				{
					totalWidth += letterSpacing * size;
				}
				else
				{
					currentChild++;
					int index = font.CharSequence.IndexOf(text[i]);
					var sprite = font.CharSprites[index];

					// calculate char particle size ratio
					var ratio = size * sprite.rect.width / 50f;

					// calculate char position
					totalWidth += font.CharKerningOffsets[index].pre * 0.01f * size;
					var position = (totalWidth-offset)/ratio;
					float charWidth = sprite.rect.width + font.CharKerningOffsets[index].post;
					totalWidth += (charWidth * 0.01f + letterSpacing) * size;

					// update particle system for this letter
					var letterObj = this.transform.GetChild(currentChild).gameObject;
					letterObj.name = letter.ToString();
					var particleSystem = letterObj.GetComponent<ParticleSystem>();

					var mainModule = particleSystem.main;
					mainModule.startSizeXMultiplier = baseScaleX * ratio;
					mainModule.startSizeYMultiplier = baseScaleY * ratio;
					mainModule.startSizeZMultiplier = baseScaleZ * ratio;

					particleSystem.textureSheetAnimation.SetSprite(0, sprite);

					mainModule.startRotation = Mathf.Deg2Rad * rotation;
					mainModule.startColor = backgroundColor;

					var customData = particleSystem.customData;
					customData.enabled = true;
					customData.SetColor(ParticleSystemCustomData.Custom1, color1);
					customData.SetColor(ParticleSystemCustomData.Custom2, color2);

					if (cumulativeDelay)
					{
						mainModule.startDelay = delay * i;
						mainModule.startLifetime = Mathf.LerpUnclamped(baseLifetime, baseLifetime + (delay * (text.Length-i)), compensateLifetime);
					}
					else
					{
						mainModule.startDelay = delay;
					}
					mainModule.startLifetime = mainModule.startLifetime.constant * lifetimeMultiplier;

					// particle system renderer parameters
					var particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
					particleRenderer.enabled = true;
					particleRenderer.pivot = new Vector3(basePivot.x + position, basePivot.y, basePivot.z);
					particleRenderer.sortingFudge += i * sortingFudgeOffset;
				}
			}

			// set active state for needed letter only
			for (int i = 1, l = this.transform.childCount; i < l; i++)
			{
				this.transform.GetChild(i).gameObject.SetActive(i <= charCount);
			}

			// play all
			this.GetComponent<ParticleSystem>().Play(true);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(CFXR_ParticleText_Runtime))]
	public class ParticleTextRuntimeEditor : Editor
	{
		CFXR_ParticleText_Runtime castTarget { get { return (CFXR_ParticleText_Runtime)this.target; } }

		GUIContent UpdateTextLabel = new GUIContent(" Update Text ", "Regenerate the text and create new letter GameObjects if needed.");

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

			if (GUILayout.Button(UpdateTextLabel, GUILayout.Height(30)))
			{
				castTarget.InitializeFirstParticle();
				castTarget.GenerateText(castTarget.text);
			}

			GUILayout.EndHorizontal();
		}
	}
#endif
}