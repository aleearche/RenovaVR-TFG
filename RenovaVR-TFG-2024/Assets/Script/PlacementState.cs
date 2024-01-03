using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    ObjectsDatabaseSO database;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;
    public ObjectInstantiator objectInstantiator;

    public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          ObjectsDatabaseSO database,
                          GridData floorData,
                          GridData furnitureData,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
        }
        else
            throw new System.Exception($"No object with ID {iD}");

        // Suscribirse al evento OnObjectPlaced para manejar la colocación de objetos
        objectPlacer.OnObjectPlaced += HandleObjectPlaced;

    }

    public void EndState()
    {
        // Detener la suscripción al evento al finalizar el estado
        objectPlacer.OnObjectPlaced -= HandleObjectPlaced;

        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        Debug.Log("OnAction called with gridPosition: " + gridPosition);
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        Debug.Log("Placement validity: " + placementValidity);

        if (placementValidity == false)
        {
            Debug.Log("Placement is not valid at position: " + gridPosition);
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }

        // Llama a una función de comando para realizar la colocación en la red
        objectPlacer.PlaceObject(selectedObjectIndex, grid.CellToWorld(gridPosition));
        Debug.Log("Placed object at position: " + gridPosition);

        // Verifica si el objeto se ha agregado correctamente en la lista de objetos colocados
        if (objectPlacer.placedGameObjects.Count > 0)
        {
            GameObject placedObject = objectPlacer.placedGameObjects[objectPlacer.placedGameObjects.Count - 1];
            Debug.Log("Object added to the placed objects list: " + placedObject.name);
        }
        else
        {
            Debug.LogError("Failed to add object to placed objects list.");
        }

        soundFeedback.PlaySound(SoundType.Place);

        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
            floorData :
            furnitureData;

        selectedData.AddObjectAt(gridPosition,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);
    }

    // Esta función se llama cuando se coloca un objeto en el servidor
    private void HandleObjectPlaced(GameObject obj, Vector3 position)
    {
        objectInstantiator.InstantiateObject(obj, position, Quaternion.identity);
        Debug.Log("Instantiated object on the server at position: " + position);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
            floorData :
            furnitureData;

        bool isValid = selectedData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
        //Debug.Log("Placement validity at position " + gridPosition + ": " + isValid);
        return isValid;
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
    }
}
