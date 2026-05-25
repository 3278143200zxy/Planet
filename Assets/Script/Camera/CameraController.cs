using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public Camera mainCamera
    {
        get { return Camera.main; }
    }
    public List<Camera> cameras = new List<Camera>();
    public Planet centerPlanet;

    public float moveVelocity;
    public float angularVelocity;
    public float zoomVelocity;

    public float creatureMoveLerpVelocity;
    public float creatureCameraSize;
    public float creatureZoomVelocity;

    public float maxCameraSize;
    public float minCameraSize;
    public Slider cameraSizeSlider;

    public float maxRadiusCameraPos;
    public float minRadiusCameraPos;
    public Slider radiusCameraPosSlider;

    private bool isPushingIn = false;
    private Vector3 pushInPos;
    public float pushInVelocity;
    //public float pushInZoomVelocity;
    public float pushInSize;

    private Planet planet;
    private void Awake()
    {
        instance = this; 
        
        radiusCameraPosSlider.value = (Vector2.Distance(transform.position, Vector2.zero) - minRadiusCameraPos) / (maxRadiusCameraPos - minRadiusCameraPos);

        //Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("Light");
    }
    // Start is called before the first frame update
    void Start()
    {
        planet = MouseManager.instance.planets[0];
    }

    // Update is called once per frame
    void Update()
    {
        //Creature creature = MouseManager.instance.creature;
        //if (creature == null)
        //{
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) isPushingIn = false;
        if (Input.GetKey(KeyCode.D)) transform.RotateAround(centerPlanet.transform.position, Vector3.back, angularVelocity * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) transform.RotateAround(centerPlanet.transform.position, -Vector3.back, angularVelocity * Time.deltaTime);

        if (Input.GetKey(KeyCode.W)) transform.position += transform.up * moveVelocity * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) transform.position -= transform.up * moveVelocity * Time.deltaTime;
        if (Vector2.Distance(transform.position, Vector2.zero) > maxRadiusCameraPos) transform.position = transform.up * maxRadiusCameraPos;
        if (Vector2.Distance(transform.position, Vector2.zero) < minRadiusCameraPos) transform.position = transform.up * minRadiusCameraPos;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) radiusCameraPosSlider.value = (Vector2.Distance(transform.position, Vector2.zero) - minRadiusCameraPos) / (maxRadiusCameraPos - minRadiusCameraPos);
        transform.position = radiusCameraPosSlider.value * (maxRadiusCameraPos - minRadiusCameraPos) * transform.up + minRadiusCameraPos * transform.up;
        //if (Vector3.Dot(transform.up, transform.position) < 0) transform.position = new Vector3(0, 0, -10);
        if (!(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].orthographicSize += scroll * zoomVelocity;
                cameras[i].orthographicSize = Mathf.Min(maxCameraSize, cameras[i].orthographicSize);
                cameras[i].orthographicSize = Mathf.Max(minCameraSize, cameras[i].orthographicSize);
                cameraSizeSlider.value = (cameras[i].orthographicSize - minCameraSize) / (maxCameraSize - minCameraSize);
            }
        }
        for (int i = 0; i < cameras.Count; i++) cameras[i].orthographicSize = cameraSizeSlider.value * (maxCameraSize - minCameraSize) + minCameraSize;

        if (isPushingIn)
        {
            float objectDistance = Vector3.Distance(transform.position, pushInPos);
            float cameraDistance = Mathf.Abs(Camera.main.orthographicSize - pushInSize);

            float pushInZoomVelocity = pushInVelocity * (objectDistance / cameraDistance);

            transform.position = Vector3.Lerp(transform.position, pushInPos, Time.deltaTime * pushInVelocity);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, pushInSize, Time.deltaTime * pushInZoomVelocity);
            if (Vector3.Distance(transform.position, pushInPos) <= 0.1f) isPushingIn = false;
        }

        Vector2 dir = transform.position - planet.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        /*
    }
    else
    {
        transform.position = Vector3.Lerp(transform.position, creature.transform.position, Time.deltaTime * creatureMoveLerpVelocity);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, creatureCameraSize, creatureZoomVelocity) * Time.deltaTime;
    }
        */
    }
    public void PushIn(Vector3 pos)
    {
        isPushingIn = true;
        pos.z = -10;
        pushInPos = pos;
    }
}
