using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class InputManager : NetworkBehaviour
{
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private LayerMask placementLayermask;
    public event Action OnClicked, OnExit;

    [SerializeField] private NetworkObjectSpawner objectSpawner;
    [SerializeField] private ObjectPlacer objectPlacer;

    private void Update()
    {
        //Debug.Log("InputManager Update");
        if (!isOwned)
        {
            //Debug.Log("No tines autoridad de cliente");
            //Debug.Log($"Cliente actual: {connectionToClient}");
            //Debug.Log($"Autoridad esperada: {objectPlacer.connectionToClient}");
            //return; // Salir si no tienes autoridad de cliente
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Obtén la posición del mapa en la que se hizo clic
            Vector3 clickPosition = GetSelectedMapPosition();

            // Llama a la función GetSelectedPrefab para obtener el prefab seleccionado.
            GameObject prefabToSpawn = GetSelectedPrefab();

            // Verifica si se encontró un prefab válido.
            if (prefabToSpawn != null)
            {
                // Obtén el índice del prefab en la lista de prefabsToSpawn del NetworkObjectSpawner.
                int prefabIndex = objectSpawner.GetPrefabIndex(prefabToSpawn);

                // Llama a la función SpawnObject del NetworkObjectSpawner con el índice y la posición adecuados.
                objectSpawner.SpawnObject(prefabIndex, clickPosition);
            }
            else
            {
                // Maneja el caso en el que no se haya seleccionado ningún prefab.
                Debug.Log("No se ha seleccionado ningún prefab.");
            }

            // Agrega esta línea para mostrar que se llama a PlaceStructure
            Debug.Log("InputManager - PlaceStructure called");

            OnClicked?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            OnExit?.Invoke();
    }


    private void HandleMouseClick()
    {
        if (IsPointerOverUI())
        {
            return;
        }

        // Obtén la posición del mapa en la que se hizo clic
        Vector3 clickPosition = GetSelectedMapPosition();

        // Llama a la función GetSelectedPrefab para obtener el prefab seleccionado.
        GameObject prefabToSpawn = GetSelectedPrefab();

        // Verifica si se encontró un prefab válido.
        if (prefabToSpawn != null)
        {
            // Obtén el índice del prefab en la lista de prefabsToSpawn del NetworkObjectSpawner.
            int prefabIndex = objectSpawner.GetPrefabIndex(prefabToSpawn);

            // Llama a la función SpawnObject del NetworkObjectSpawner con el índice y la posición adecuados.
            objectSpawner.SpawnObject(prefabIndex, clickPosition);
            OnClicked?.Invoke();
        }
        else
        {
            // Maneja el caso en el que no se haya seleccionado ningún prefab.
            Debug.Log("No se ha seleccionado ningún prefab.");
        }
    }

    public bool IsPointerOverUI()
    {
        Debug.Log("Entro");
        return EventSystem.current.IsPointerOverGameObject();
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, placementLayermask))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    public GameObject GetSelectedPrefab()
    {
        // Rayo desde la posición del ratón hacia el mundo en la dirección de la cámara.
        Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, placementLayermask))
        {
            // Verificar si el objeto golpeado tiene un componente NetworkObjectSpawner.
            NetworkObjectSpawner spawner = hit.collider.GetComponent<NetworkObjectSpawner>();

            if (spawner != null && spawner.prefabsToSpawn.Count > 0)
            {
                // Devolver el primer prefab de la lista de prefabsToSpawn del NetworkObjectSpawner.
                return spawner.prefabsToSpawn[0];
            }
        }

        return null; // Devuelve null si no se seleccionó ningún prefab.
    }
}
