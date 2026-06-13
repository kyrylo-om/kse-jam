using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Info")]
    [SerializeField] private string blockName = "Generic Block";

    public string BlockName => blockName;

    protected virtual void Awake()
    {
        // Base initialization if needed
    }
}
