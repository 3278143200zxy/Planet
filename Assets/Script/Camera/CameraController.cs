using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private bool isPushingIn = false;
    private Vector3 pushInPos;
    public float pushInVelocity;
    //public float pushInZoomVelocity;
    public float pushInSize;

    private Planet planet;
    private void Awake()
    {
        instance = this;

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
        if (Input.GetKey(KeyCode.W)) transform.position += transform.up * moveVelocity * Time.deltaTime;//transform.position.normalized * moveVelocity * Time.deltaTime;
        if (Input.GetKey(KeyCode.S) && Vector2.Distance(transform.position, planet.transform.position) >= 2f) transform.position -= transform.up * moveVelocity * Time.deltaTime;//transform.position.normalized * moveVelocity * Time.deltaTime;
        //if (Vector3.Dot(transform.up, transform.position) < 0) transform.position = new Vector3(0, 0, -10);
        if (!(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].orthographicSize += scroll * zoomVelocity;
                cameras[i].orthographicSize = Mathf.Max(maxCameraSize, cameras[i].orthographicSize);
            }
        }
        
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
