using UnityEngine;

public class FingerScript2 : MonoBehaviour
{
    public Transform springObject;    // ばねオブジェクト
    public float moveSpeed = 5f;      // 指の移動速度
    private Rigidbody fingerRigidbody;

    void Start()
    {
        fingerRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        MoveFinger();
    }

    void MoveFinger()
    {
        // キーボード入力で指を上下に動かす
        float moveDirection = Input.GetAxis("Vertical");  // 上下矢印キーで動く
        Vector3 move = new Vector3(0, moveDirection * moveSpeed * Time.deltaTime, 0);

        // 指オブジェクトの位置を更新
        fingerRigidbody.MovePosition(transform.position + move);
    }
}
