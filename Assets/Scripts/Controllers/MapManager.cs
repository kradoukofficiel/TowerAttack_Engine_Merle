using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/* Controller Global du System de Map
 * Contient les references vers les Datas de la map (MapData) 
 * Contient les references vers les vues de la map
 */
public class MapManager : MonoBehaviour
{
    // Variable de Data.
    public MapData mapData;

    // Variables de vues & NavMesh.
    [Header("View Var")]
    public bool generateView = true;
    public GameObject prefabSurface;
    public GameObject navContainer;
    public GameObject prefabWall, prefabWater;

    // Variables de debug.
    [Header("Debug Var")]
    [Range(0.0f,0.5f)]
    public float downScale = 0.1f;
    public bool showDebugGrid = true;

    [ContextMenu("InitializeMap")]
    public void InitializeMap()
    {
        // Intialization des Datas.
        mapData.grid = new SquareData[mapData.width * mapData.height];

        // Parcours des square pour générer aléatoirement un state.
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            int valMin = (int)SquareState.Normal;
            int valMax = (int)SquareState.Special;
            mapData.grid[i].state = (SquareState)Random.Range(valMin, valMax + 1);
        }

        // Clean NavContainer
        DestroyAllChild(navContainer);

        if(generateView)
        {
            // Instantiation des view des squares en fonction des Datas.
            CreateSquaresView();

            // Creation de la surface en fonction des Datas.
            CreateSurface();
        }
    }

    // Permet de créer automatiquement la surface du navmesh à partir des données de la map
    private void CreateSurface()
    {
        // Création de la vue en fonction des Datas
        //GameObject surface = Instantiate(prefabSurface);
        GameObject surface = (GameObject)PrefabUtility.InstantiatePrefab(prefabSurface);
        surface.transform.SetParent(navContainer.transform);

        // Calcul de la position de la surface.
        Vector3 posSurface = surface.transform.position;
        posSurface.x = mapData.width / 2;
        posSurface.z = mapData.height / 2;

        // Test si width impair pour ajout de declage sur la position.
        if (mapData.width % 2 != 0)
        {
            posSurface.x += 0.5f;
        }
        if (mapData.height % 2 != 0)
        {
            posSurface.z += 0.5f;
        }
        surface.transform.position = posSurface;

        // Calcul du scale de la surface.
        Vector3 scaleSurface = surface.transform.localScale;
        scaleSurface.x = mapData.width;
        scaleSurface.z = mapData.height;
        surface.transform.localScale = scaleSurface;

        // Bake NavMesh
        NavMeshSurface surfaceComponent = surface.GetComponent<NavMeshSurface>();
        surfaceComponent.BuildNavMesh();
    }

    // Permet de créer la vue des squares en fonction de leur state.
    private void CreateSquaresView()
    {
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            // Creation d'une prefab de vue en focntion du state.
            GameObject newSquareView = null;
            switch(mapData.grid[i].state)
            {
                case SquareState.Lock:
                    newSquareView = (GameObject)PrefabUtility.InstantiatePrefab(prefabWall);
                    break;
                case SquareState.Water:
                    newSquareView = (GameObject)PrefabUtility.InstantiatePrefab(prefabWater);
                    break;
            }

            // Placement de la vue.
            if(newSquareView != null)
            {
                newSquareView.transform.SetParent(navContainer.transform);
                Vector3 newPosSquare = PosSquareFromIndex(i);
                newPosSquare.x += 0.5f;
                newPosSquare.z += 0.5f;
                newSquareView.transform.position = newPosSquare;
            }
        }
    }

    private Vector3 PosSquareFromIndex(int i)
    {
        Vector3 pos = Vector3.zero;
        // Calcul de la position en x => le reste de la division
        pos.x = i % mapData.width;
        // Calcul de la position en z => partie entière de la division  
        pos.z = i / mapData.width;
        return pos;
    }

    private void DestroyAllChild(GameObject parent)
    {
        if (parent != null)
        {
            // Mode editeur pas : Destroy(parent);
            // Il faut utiliser : DestroyImmediate();
            for (int i = parent.transform.childCount; i > 0; i--)
            {
                DestroyImmediate(parent.transform.GetChild(0).gameObject);
            }
        }

    }
    
    private void OnDrawGizmos()
    {
        if(showDebugGrid)
        {
            ShowGizmoMapSquareData();
        }
    }

    private void ShowGizmoMapSquareData()
    {
        Vector3 pos = Vector3.zero;
        // Parcours des élements du tableau via un for.
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            // Calcul des coordonnées du square dans la grid à partir de l'index dand le tableau.
            // Calcul de la position en x => le reste de la division
            pos.x = i % mapData.width;
            // Calcul de la position en z => partie entière de la division  
            pos.z = i / mapData.width;

            Gizmos.color = GetColorFromState(mapData.grid[i].state);
            //Gizmos.DrawCube(pos, (Vector3.one) / 2);

            // Affichage 1ere ligne
            Vector3 posFrom = pos;
            posFrom.x += downScale;
            posFrom.z += downScale;
            Vector3 posTo = pos;
            posTo.x += 1 - downScale;
            posTo.z += downScale;
            Gizmos.DrawLine(posFrom, posTo);

            // Affichage 2eme ligne
            posFrom = pos;
            posTo = pos;
            posFrom.x += 1 - downScale;
            posFrom.z += downScale;
            posTo.x += 1 - downScale;
            posTo.z += 1 - downScale;
            Gizmos.DrawLine(posFrom, posTo);

            // Affichage 3eme ligne
            posFrom = pos;
            posTo = pos;
            posFrom.x += 1 - downScale;
            posFrom.z += 1 - downScale;
            posTo.x += downScale;
            posTo.z += 1 - downScale;
            Gizmos.DrawLine(posFrom, posTo);

            // Affichage 4eme ligne
            posFrom = pos;
            posTo = pos;
            posFrom.x += downScale;
            posFrom.z += 1 - downScale;
            posTo.x += downScale;
            posTo.z += downScale;
            Gizmos.DrawLine(posFrom, posTo);
        }
    }

    private Color GetColorFromState(SquareState state)
    {
        switch (state)
        {
            case SquareState.Lock:
                return Color.black;
            case SquareState.Grass:
                return Color.green;
            case SquareState.Water:
                return Color.blue;
            case SquareState.Special:
                return Color.magenta;
            default:
                return Color.white;
        }
    }
}
