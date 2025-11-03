using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRigidbody : MonoBehaviour
{
    private Planet planet;
    private BaseUnit baseUnit;

    public float gravity = 9.8f;
    public Vector3 velocity;

    // Start is called before the first frame update
    private void Awake()
    {
        baseUnit = GetComponent<BaseUnit>();
    }
    void Start()
    {
        planet = MouseManager.instance.planets[0];
    }

    // Update is called once per frame
    void Update()
    {
        velocity -= (transform.position - planet.transform.position).normalized * gravity * Time.deltaTime;

    }
    private void LateUpdate()
    {
        //Debug.Log(velocity);
        transform.position += velocity * Time.deltaTime;
    }
}
