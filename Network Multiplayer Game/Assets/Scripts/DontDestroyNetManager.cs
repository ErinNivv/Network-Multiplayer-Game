using UnityEngine;

public class DontDestroyNetManager : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"[DontDestroy] Awake called on {gameObject.name} in scene {gameObject.scene.name}");
        DontDestroyOnLoad(gameObject);
    }
}
