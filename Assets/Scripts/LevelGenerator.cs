using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("The total number of blocks to generate in the level.")]
    [SerializeField] private int levelLength = 20;

    [Tooltip("The size/length of each block along the Z-axis (spacing center-to-center).")]
    [SerializeField] private float blockLength = 4f;

    [Tooltip("List of block prefabs to choose randomly from.")]
    [SerializeField] private GameObject[] blockPrefabs;

    [Tooltip("Automatically generate the level on Awake when playing.")]
    [SerializeField] private bool generateOnAwake = true;

    [Header("Hierarchy Reference")]
    [Tooltip("Transform parent under which blocks will be generated (optional).")]
    [SerializeField] private Transform levelContainer;

    private void Awake()
    {
        if (generateOnAwake)
        {
            GenerateLevel();
        }
    }

    public void GenerateLevel()
    {
        ClearLevel();

        if (blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogWarning("No block prefabs assigned to the LevelGenerator!");
            return;
        }

        // Create container if not set
        if (levelContainer == null)
        {
            GameObject containerObj = new GameObject("GeneratedLevel");
            containerObj.transform.SetParent(transform);
            containerObj.transform.localPosition = Vector3.zero;
            levelContainer = containerObj.transform;
        }

        for (int i = 0; i < levelLength; i++)
        {
            // Select a random block prefab
            GameObject prefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];
            if (prefab == null) continue;

            // Position each block sequentially along the Z-axis starting from Z = 0
            Vector3 spawnPosition = new Vector3(0f, -0.5f, i * blockLength);

            GameObject spawnedBlock;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                spawnedBlock = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (spawnedBlock != null)
                {
                    spawnedBlock.transform.position = spawnPosition;
                    spawnedBlock.transform.SetParent(levelContainer);
                    Undo.RegisterCreatedObjectUndo(spawnedBlock, "Generate Block");
                }
            }
            else
#endif
            {
                spawnedBlock = Instantiate(prefab, spawnPosition, Quaternion.identity, levelContainer);
            }

            if (spawnedBlock != null)
            {
                spawnedBlock.name = $"Block_{i}_{prefab.name}";
            }
        }

        Debug.Log($"Successfully generated level with {levelLength} blocks.");
    }

    public void ClearLevel()
    {
        if (levelContainer != null)
        {
            // Destroy all children of the container
            int childCount = levelContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject child = levelContainer.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.DestroyObjectImmediate(child);
                }
                else
#endif
                {
                    Destroy(child);
                }
            }
        }
        else
        {
            // Find existing child with the container name
            Transform existingContainer = transform.Find("GeneratedLevel");
            if (existingContainer != null)
            {
                levelContainer = existingContainer;
                ClearLevel();
            }
        }
    }
}
