using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Player : MonoBehaviour
{
    public float speed = 10.0f;
    Animator a;
    SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        a  = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal_move = Input.GetAxis("Horizontal");
        float vertical_move   = Input.GetAxis("Vertical");
        if (horizontal_move == 0) {
            a.SetBool("HorWalk", false);
        } else {
            a.SetBool("HorWalk", true);
            if (horizontal_move > 0) {
                if (!sr.flipX) {
                    sr.flipX = true;
                }
            } else if (horizontal_move < 0) {
                if (sr.flipX) {
                    sr.flipX = false;
                }
            }
        }
        if (vertical_move == 0) {
            a.SetBool("VerWalkDown", false);
            a.SetBool("VerWalkUp", false);
        } else {
            if (vertical_move > 0) {
                a.SetBool("VerWalkUp", true);
            } else {
                a.SetBool("VerWalkDown", true);
            }
        }
        transform.position += new Vector3(horizontal_move, vertical_move, 0) * speed * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) && GameEvents.Ins.OnPlayerActionKey != null) {
            GameEvents.Ins.OnPlayerActionKey();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Object")
        {
            if (Input.GetKey("e"))
            {
                Debug.Log("Interactuo?");
            }
        }
    }


}
