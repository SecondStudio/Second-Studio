using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (GetComponent<PhotonView>().isMine)
        {
            InputMovement();
        }
    }

    void InputMovement()
    {
        if (Input.GetKey(KeyCode.W))
            rb.MovePosition(rb.position + Vector3.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.S))
            rb.MovePosition(rb.position - Vector3.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            rb.MovePosition(rb.position + Vector3.right * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.A))
            rb.MovePosition(rb.position - Vector3.right * speed * Time.deltaTime);
    }
}