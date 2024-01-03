using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObjectPlacer : NetworkBehaviour
{
    [SerializeField] public List<GameObject> placedGameObjects = new List<GameObject>();
    [SerializeField] private BudgetManager budgetManager; // Reference to the BudgetManager component
    [SerializeField] private NetworkObjectSpawner objectSpawner; // Reference to the NetworkObjectSpawner component

    // Declaración del delegado con dos argumentos
    public delegate void ObjectPlacedDelegate(GameObject obj, Vector3 position);

    // Declaración del evento utilizando el delegado 
    public event ObjectPlacedDelegate OnObjectPlaced;

    [Command]
    public void PlaceObject(int prefabIndex, Vector3 position)
    {
        Debug.Log("PlaceObject");
        // Verificar si el cliente tiene autoridad sobre el objeto
        if (!HasClientAuthority(prefabIndex))
        {
            Debug.Log("Client does not have authority to place the object.");
            return;
        }

        if (!budgetManager.CanAfford())
        {
            Debug.Log("Not enough budget to place the object.");
            return;
        }

        if (prefabIndex < 0 || prefabIndex >= objectSpawner.prefabsToSpawn.Count)
        {
            Debug.Log("Invalid prefab index: " + prefabIndex);
            return;
        }

        GameObject prefabToSpawn = objectSpawner.prefabsToSpawn[prefabIndex];
        Debug.Log("Spawning prefab: " + prefabToSpawn.name + " at position: " + position);
        GameObject newObject = Instantiate(prefabToSpawn, position, Quaternion.identity);
        NetworkServer.Spawn(newObject); // Sincronizar la instanciación en la red

        budgetManager.RemoveBudget(); // Deduct the cost from the budget
        RpcNotifyObjectPlaced(newObject, position); // Notificar a los clientes sobre la colocación del objeto
    }

    [Command]
    public void RemoveObjectAt(int gameObjectIndex)
    {
        if (gameObjectIndex < 0 || gameObjectIndex >= placedGameObjects.Count)
        {
            Debug.Log("Invalid gameObjectIndex: " + gameObjectIndex);
            return;
        }

        GameObject objectToRemove = placedGameObjects[gameObjectIndex];
        if (objectToRemove != null)
        {
            Debug.Log("Removing object: " + objectToRemove.name);
            NetworkServer.Destroy(objectToRemove); // Destruye el objeto en el servidor
            budgetManager.AddBudget(); // Agrega el costo nuevamente al presupuesto cuando se elimina un objeto
            placedGameObjects[gameObjectIndex] = null; // Actualiza la lista de objetos colocados en todos los clientes
        }
    }

    [ClientRpc]
    public void RpcNotifyObjectPlaced(GameObject obj, Vector3 position)
    {
        Debug.Log("Object placed on clients: " + obj.name + " at position: " + position);
        // Llamar al evento para notificar a los clientes
        OnObjectPlaced?.Invoke(obj, position);
    }

    public bool HasClientAuthority(int prefabIndex)
    {
        if (objectSpawner != null && prefabIndex >= 0 && prefabIndex < objectSpawner.prefabsToSpawn.Count)
        {
            GameObject prefabToSpawn = objectSpawner.prefabsToSpawn[prefabIndex];
            NetworkIdentity networkIdentity = prefabToSpawn.GetComponent<NetworkIdentity>();

            bool hasAuthority = networkIdentity != null && networkIdentity.isOwned;
            Debug.Log("Client has authority for prefab: " + prefabToSpawn.name + " - HasAuthority: " + hasAuthority);
            return hasAuthority;
        }
        return false;
    }
}
