using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Extensions;
using Assets.Scripts.ReactiveEffects.Base;
using UnityEngine;
using UnityEngine.UI;

public class UIPrefabLayoutAudioObject : VisualizationEffectBase
{
    #region Private Member Variables

    private List<GameObject> _gameObjects = new List<GameObject>();

    #endregion

    #region Public Properties

    public GameObject Prefab;
    public PrefabLayoutType LayoutType;
    public float ObjectWidthDepth;
    public float CircularLayoutRadius;
    public Vector3 ObjectRotation;
    public Vector3 RotationOffset;
    public bool Shuffle;
    public int DesiredColorGroupCount;

    #endregion

    #region Startup / Shutdown

    public override void Start()
    {
        base.Start();

        int groupSize = LoopbackAudio.SpectrumSize / DesiredColorGroupCount;

        // Instantiate GameObjects
        for (int i = 0; i < LoopbackAudio.SpectrumSize; i++)
        {
            GameObject newGameObject = Instantiate(Prefab, gameObject.transform, false);

            _gameObjects.Add(newGameObject);

            int group = (i / groupSize);

            Image rend = newGameObject.GetComponent<Image>();

            Color color = Globals.PastelColors[group];
            rend.color = color;

            // Try to set various other used scripts
            VisualizationEffectBase[] visualizationEffects = newGameObject.GetComponents<VisualizationEffectBase>();

            if (visualizationEffects != null && visualizationEffects.Length > 0)
            {
                foreach (VisualizationEffectBase visualizationEffect in visualizationEffects)
                {
                    visualizationEffect.AudioVisualizationStrategy = AudioVisualizationStrategy;
                    visualizationEffect.AudioSampleIndex = i;
                }
            }
        }

        _performLayout();
    }

    #endregion

    #region Render

    public void Update()
    {
#if DEBUG

        if (Input.GetKeyDown(KeyCode.L))
        {
            _performLayout();
        }

#endif
    }

    #endregion

    #region Private Methods

    private void _performLayout()
    {
        List<Vector2> layoutPositions = new List<Vector2>();

        RectTransform parentRect = GetComponent<RectTransform>();

        switch (LayoutType)
        {
            case PrefabLayoutType.XLinear:

                float totalWidth = LoopbackAudio.SpectrumData.Length * ObjectWidthDepth;
                float startX = -totalWidth / 2f;

                for (int i = 0; i < LoopbackAudio.SpectrumData.Length; i++)
                {
                    float x = startX + i * ObjectWidthDepth;
                    layoutPositions.Add(new Vector2(x, 0f));
                }

                break;

            case PrefabLayoutType.XZSpread:

                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(LoopbackAudio.SpectrumData.Length));
                float half = (gridSize * ObjectWidthDepth) / 2f;

                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        if (layoutPositions.Count >= LoopbackAudio.SpectrumData.Length)
                            break;

                        float posX = (x * ObjectWidthDepth) - half;
                        float posY = (y * ObjectWidthDepth) - half;

                        layoutPositions.Add(new Vector2(posX, posY));
                    }
                }

                if (Shuffle)
                    layoutPositions.Shuffle();

                break;

            case PrefabLayoutType.XZCircular:

                for (int i = 0; i < LoopbackAudio.SpectrumData.Length; i++)
                {
                    float angle = (float)i / LoopbackAudio.SpectrumData.Length * Mathf.PI * 2f;

                    float x = Mathf.Cos(angle) * CircularLayoutRadius;
                    float y = Mathf.Sin(angle) * CircularLayoutRadius;

                    layoutPositions.Add(new Vector2(x, y));
                }

                if (Shuffle)
                    layoutPositions.Shuffle();

                break;
        }
        
        for (int i = 0; i < layoutPositions.Count; i++)
        {
            RectTransform rect = _gameObjects[i].GetComponent<RectTransform>();

            rect.anchoredPosition = layoutPositions[i];
            rect.localRotation = Quaternion.Euler(ObjectRotation);
        }

        parentRect.localRotation *= Quaternion.Euler(RotationOffset);
    }

    #endregion
}