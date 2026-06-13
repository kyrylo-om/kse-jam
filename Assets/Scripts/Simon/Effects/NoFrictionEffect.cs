using UnityEngine;

public class NoFrictionEffect : ISimonEffect
{
    private GameObject playerObject;
    private CapsuleCollider playerCollider;
    private PhysicsMaterial originalMaterial;
    private PhysicsMaterial zeroFrictionMaterial;
    private bool applied;

    public NoFrictionEffect(float _)
    {
        // floatValue is unused for this effect
    }

    public void Apply()
    {
        playerObject = GameObject.Find("PlayerBean");
        if (playerObject == null)
        {
            Debug.LogWarning("[NoFrictionEffect] PlayerBean not found!");
            return;
        }

        playerCollider = playerObject.GetComponent<CapsuleCollider>();
        if (playerCollider == null) return;

        // Save the original material
        originalMaterial = playerCollider.sharedMaterial;

        // Create a zero-friction material
        zeroFrictionMaterial = new PhysicsMaterial("ZeroFriction");
        zeroFrictionMaterial.dynamicFriction = 0f;
        zeroFrictionMaterial.staticFriction = 0f;
        zeroFrictionMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;

        playerCollider.sharedMaterial = zeroFrictionMaterial;
        applied = true;
        Debug.Log("[NoFrictionEffect] Friction removed from player!");
    }

    public void Reset()
    {
        if (!applied || playerCollider == null) return;

        playerCollider.sharedMaterial = originalMaterial;

        // Clean up the temporary material
        if (zeroFrictionMaterial != null)
        {
            Object.Destroy(zeroFrictionMaterial);
            zeroFrictionMaterial = null;
        }

        applied = false;
        Debug.Log("[NoFrictionEffect] Friction restored.");
    }
}