using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Qual è il grado di rotazione attuale
    private float curXRot;
    // Qual è il valore di zoom attuale
    private float curZoom;
    private Camera cam;

    [SerializeField] private CameraSettings _settings;

    void Start()
    {
        cam = Camera.main;
        curZoom = cam.transform.localPosition.y;
        curXRot = -50;
    }

    void Update()
    {
        // Zooming
        Zooming();

        // Rotating
        Rotating();

        // Moving
        Moving();

    }

    private void Zooming()
    {
        // Zooming
        /*
        Dobbiamo aggiornare il valore curZoom accedendo alla rotella del mouse. 
        La funzione Input.GetAxis permette di ottenere il valore della rotella, 
        moltiplicato per zoomSpeed invertito — in modo da sommare quando si zooma indietro, e sottrarre quando si zooma in avanti.
        */
        curZoom += Input.GetAxis("Mouse ScrollWheel") * -_settings.zoomSpeed;
        // Mathf.Clamp viene usato per mantenere curZoom tra minZoom e maxZoom.
        curZoom = Mathf.Clamp(curZoom, _settings.minZoom, _settings.maxZoom);
        // Infine, applichiamo il valore alla posizione effettiva della camera moltiplicando l'asse Y locale (Vector3.up) per curZoom.
        cam.transform.localPosition = Vector3.up * curZoom;
    }

    private void Rotating()
    {
        if (Input.GetMouseButton(1))
        {
            // otteniamo la posizione del mouse (x, y) tramite Input.GetAxis.
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");

            curXRot += -y * _settings.rotateSpeed;
            curXRot = Mathf.Clamp(curXRot, _settings.minXRot, _settings.maxXRot);

            transform.eulerAngles = new Vector3(curXRot, transform.eulerAngles.y + (x * _settings.rotateSpeed), .0f);
        }
    }

    private void Moving()
    {
        Vector3 forward = cam.transform.forward;
        forward.y = .0f;
        forward.Normalize();

        Vector3 right = cam.transform.right;
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 dir = forward * moveZ + right * moveX;
        dir.Normalize();

        dir *= _settings.moveSpeed * Time.deltaTime;
        transform.position += dir;
    }
}
