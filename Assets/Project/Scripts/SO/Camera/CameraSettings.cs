using UnityEngine;

[CreateAssetMenu(fileName = "Camera Settings", menuName = "Camera")]
public class CameraSettings : ScriptableObject
{
    // A che velocità si muove la camera
    public float moveSpeed;
    // Qual è la rotazione min/max per guardare su e giù
    public float minXRot;
    public float maxXRot;
    // Quanto possiamo zoomare in/out
    public float minZoom;
    public float maxZoom;
    // A che velocità possiamo ruotare e/o zoomare
    public float zoomSpeed;
    public float rotateSpeed;
}