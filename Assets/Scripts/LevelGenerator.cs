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

    [Header("Barriers")]
    [Tooltip("Vertical panel spawned every N blocks.")]
    [SerializeField] private GameObject barrierPrefab;
    [SerializeField] private int barrierBlocks = 5;

    [Header("End Block")]
    [Tooltip("Block spawned at the end of the track.")]
    [SerializeField] private GameObject endBlockPrefab;

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

            // Spawn a barrier after every 5th block (between indices 3 and 4, 8 and 9, etc.)
            if (barrierPrefab != null && i > 0 && i % barrierBlocks == 0)
            {
                Vector3 barrierPos = new Vector3(0f, 6f, i * blockLength + blockLength * 0.5f);
                SpawnBarrier(barrierPos, i);
            }
        }

        Debug.Log($"Successfully generated level with {levelLength} blocks.");

        // Spawn end block at the end of the track
        if (endBlockPrefab != null)
        {
            Vector3 endPos = new Vector3(0f, -0.5f, levelLength * blockLength);
            SpawnEndBlock(endPos);
        }
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

    private void SpawnBarrier(Vector3 position, int blockIndex)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject barrier = PrefabUtility.InstantiatePrefab(barrierPrefab) as GameObject;
            if (barrier != null)
            {
                barrier.transform.position = position;
                barrier.transform.SetParent(levelContainer);
                barrier.name = $"Barrier_{blockIndex}";
                Undo.RegisterCreatedObjectUndo(barrier, "Generate Barrier");
            }
            return;
        }
#endif
        GameObject spawnedBarrier = Instantiate(barrierPrefab, position, Quaternion.identity, levelContainer);
        spawnedBarrier.name = $"Barrier_{blockIndex}";
    }

    private void SpawnEndBlock(Vector3 position)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject endBlock = PrefabUtility.InstantiatePrefab(endBlockPrefab) as GameObject;
            if (endBlock != null)
            {
                endBlock.transform.position = position;
                endBlock.transform.SetParent(levelContainer);
                endBlock.name = "EndBlock";
                Undo.RegisterCreatedObjectUndo(endBlock, "Generate End Block");
            }
            return;
        }
#endif
        GameObject spawned = Instantiate(endBlockPrefab, position, Quaternion.identity, levelContainer);
        spawned.name = "EndBlock";
    }
}
