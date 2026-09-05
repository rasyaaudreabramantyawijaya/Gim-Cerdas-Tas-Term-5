
using UnityEngine;
 
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
 
    void Update()
    {
        // Input.GetAxis("Horizontal") -> A/D atau arrow kiri/kanan
        // Input.GetAxis("Vertical")   -> W/S atau arrow atas/bawah
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
 
        Vector3 direction = new Vector3(h, 0f, v);
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }
}
 