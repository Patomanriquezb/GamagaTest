using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rigidbody;
    public float acceleration = 1;
    public GameObject camera;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        SetPhysicsActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        rigidbody.AddForce(Vector3.forward * Input.GetAxis("Vertical")* acceleration, ForceMode.Acceleration);
        rigidbody.AddForce(Vector3.right * Input.GetAxis("Horizontal") * acceleration, ForceMode.Acceleration);
    }

    public void MoveToPoint(Vector3 point)
    {
        transform.position = point;
    }

    public void SetPhysicsActive(bool state)
    {
        rigidbody.isKinematic = !state;
    }

    public void Reset()
    {
        SetPhysicsActive(false);
        rigidbody.velocity = Vector3.zero;
    }
}
