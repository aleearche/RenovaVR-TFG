using UnityEngine;

public class ObjectInstantiator : MonoBehaviour
{
    public void InstantiateObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        Instantiate(prefab, position, rotation);
    }
}
