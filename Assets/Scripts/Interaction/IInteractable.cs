using UnityEngine;

public interface IInteractable {
    /// <summary>
    /// Called when an interactor (e.g. player) interacts with this object.
    /// </summary>
    void Interact(GameObject interactor);

    /// <summary>
    /// Returns the position used for distance checks.
    /// </summary>
    Vector3 GetPosition();
}
