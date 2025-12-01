using UnityEngine;

public enum InteractableType { Generic, Door }

public class Interactable : MonoBehaviour
{
    public InteractableType type = InteractableType.Generic;

    // For doors
    public Transform doorTarget;
}
