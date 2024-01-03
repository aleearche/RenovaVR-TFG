using System;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class NetworkObjectSpawner : NetworkBehaviour
{
    [SerializeField] public List<GameObject> prefabsToSpawn = new List<GameObject>();
    [SerializeField] private BudgetManager budgetManager; // Reference to the BudgetManager component

    public delegate void ObjectPlacedDelegate(GameObject obj, Vector3 position);
    public event ObjectPlacedDelegate OnObjectPlaced;

    public ObjectPlacer objectPlacer;

    [Command]
    public void AuthorizeAllObjects()
    {
        // Verificar si el cliente tiene autoridad sobre cada objeto en la lista
        for (int i = 0; i < prefabsToSpawn.Count; i++)
        {
            if (HasClientAuthority(i))
            {
                // Asignar autoridad al objeto actual
                AssignAuthorityToPrefab(i);
            }
        }
    }

    [Command]
    public void SpawnObject(int prefabIndex, Vector3 position)
    {
        Debug.Log("Holaaaa");
        if (budgetManager.CanAfford())
        {
            // Verificar si el cliente tiene autoridad sobre el objeto
            Debug.Log("Checking authority before spawning object.");
            if (objectPlacer != null && objectPlacer.HasClientAuthority(prefabIndex))
            {
                Debug.Log("Client has authority. Proceeding to spawn.");
                if (prefabIndex >= 0 && prefabIndex < prefabsToSpawn.Count)
                {
                    GameObject prefabToSpawn = prefabsToSpawn[prefabIndex]; // Obtén el prefab de la lista

                    // Registro: Indicar el inicio del proceso de spawn
                    Debug.Log("Spawning prefab: " + prefabToSpawn.name + " at position: " + position);

                    GameObject newObject = Instantiate(prefabToSpawn, position, Quaternion.identity);
                    NetworkServer.Spawn(newObject); // Sincronizar la instanciación en la red

                    // Registro: Indicar que el objeto ha sido spawn
                    Debug.Log("Spawned object: " + newObject.name + " at position: " + position);

                    // Deduct the cost from the budget
                    budgetManager.RemoveBudget();
                    Debug.Log("Budget deducted for placing the object.");

                    // Notificar a los clientes sobre la colocación del objeto
                    RpcNotifyObjectPlaced(newObject, position);
                    Debug.Log("Object placed on clients: " + newObject.name + " at position: " + position);
                }
                else
                {
                    Debug.Log("Prefab index is out of range.");
                }
            }
            else
            {
                Debug.Log("Client does not have authority to place the object.");
            }
        }
        else
        {
            Debug.Log("Not enough budget to place the object.");
        }
    }

    private bool CanSpawn()
    {
        return budgetManager.CanAfford();
    }

    [ClientRpc]
    private void RpcNotifyObjectPlaced(GameObject obj, Vector3 position)
    {
        OnObjectPlaced?.Invoke(obj, position);
    }

    public int GetPrefabIndex(GameObject prefab)
    {
        return prefabsToSpawn.IndexOf(prefab);
    }

    public bool HasClientAuthority(int prefabIndex)
    {
        if (objectPlacer != null && prefabIndex >= 0 && prefabIndex < prefabsToSpawn.Count)
        {
            GameObject prefabToSpawn = prefabsToSpawn[prefabIndex];
            NetworkIdentity networkIdentity = prefabToSpawn.GetComponent<NetworkIdentity>();

            // Verificar si el cliente actual tiene autoridad sobre el objeto
            bool hasAuthority = networkIdentity != null && networkIdentity.connectionToClient == connectionToClient;
            Debug.Log("Client has authority for prefab: " + prefabToSpawn.name + " - HasAuthority: " + hasAuthority);

            return hasAuthority;
        }
        return false;
    }

    public void AssignAuthorityToPrefab(int prefabIndex)
    {
        if (prefabIndex >= 0 && prefabIndex < prefabsToSpawn.Count)
        {
            GameObject prefabToSpawn = prefabsToSpawn[prefabIndex];
            NetworkIdentity networkIdentity = prefabToSpawn.GetComponent<NetworkIdentity>();

            if (networkIdentity != null)
            {
                // Asignar autoridad al cliente actual
                networkIdentity.AssignClientAuthority(connectionToClient);
                Debug.Log("Authority assigned to client for prefab: " + prefabToSpawn.name);
            }
            else
            {
                Debug.Log("NetworkIdentity not found on prefab: " + prefabToSpawn.name);
            }
        }
        else
        {
            Debug.Log("Prefab index is out of range.");
        }
    }
}
