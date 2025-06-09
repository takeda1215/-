using UnityEngine;

public class FingerScript2 : MonoBehaviour
{
    public Transform springObject;    // �΂˃I�u�W�F�N�g
    public float moveSpeed = 5f;      // �w�̈ړ����x
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
        // �L�[�{�[�h���͂Ŏw���㉺�ɓ�����
        float moveDirection = Input.GetAxis("Vertical");  // �㉺���L�[�œ���
        Vector3 move = new Vector3(0, moveDirection * moveSpeed * Time.deltaTime, 0);

        // �w�I�u�W�F�N�g�̈ʒu���X�V
        fingerRigidbody.MovePosition(transform.position + move);
    }
}
