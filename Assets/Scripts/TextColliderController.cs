using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
[RequireComponent(typeof(BoxCollider))]
public class TextColliderController : MonoBehaviour
{
    private TMP_Text tmp;
    private BoxCollider col;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        col = GetComponent<BoxCollider>();
    }

    void Update()
    {
        UpdateCollider();
    }

    void UpdateCollider()
    {
        // Forces TMP to update the mesh so bounds are accurate
        tmp.ForceMeshUpdate();

        // Get the exact bounds of the rendered text
        var bounds = tmp.textBounds;

        // Resize and reposition collider
        col.size = bounds.size;
        col.center = bounds.center;
    }
}
