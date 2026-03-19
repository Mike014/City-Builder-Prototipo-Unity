using UnityEngine;
using UnityEngine.EventSystems;

public class Selector : MonoBehaviour
{
    private Camera cam;

    // Singleton: un'unica istanza accessibile globalmente
    public static Selector instance;

    void Awake()
    {
        cam = Camera.main;
        instance = this;
    }

    void Update()
    {
        // GetCurTilePosition();
    }

    public Vector3 GetCurTilePosition()
    {
        // Se il mouse è sopra un elemento UI, ignoriamo il tile
        // e restituiamo un valore sentinella "invalido" (y = -99)
        if (EventSystem.current.IsPointerOverGameObject())
            return new Vector3(0, -99, 0);

        // Creiamo un piano orizzontale (normale Vector3.up = asse Y)
        // posizionato all'origine della scena
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        // Proiettiamo un raggio dalla camera verso la posizione del mouse
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Distanza dal punto di intersezione raggio/piano
        float rayOut = 0f;

        // Se il raggio interseca il piano...
        if (plane.Raycast(ray, out rayOut))
        {
            // Otteniamo il punto 3D di intersezione e spostiamo di -0.5 sull'asse X
            // per centrare il calcolo sulla griglia
            Vector3 newPos = ray.GetPoint(rayOut) - new Vector3(.5f, 0f, 0f);

            // CeilToInt snappa la posizione al tile corretto della griglia
            // arrotondando sempre verso l'intero superiore su X e Z
            // Y rimane 0 perché ci muoviamo solo sul piano orizzontale
            newPos = new Vector3(Mathf.CeilToInt(newPos.x), 0f, Mathf.CeilToInt(newPos.z));

            return newPos;
        }

        // Se il raggio non interseca il piano, restituiamo il valore sentinella
        return new Vector3(0, -99, 0);
    }
}