using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FingerAnimatorController : MonoBehaviour
{
    Animator anim;
    const string paramName = "BendTime";

    // 0N��1f, 10N��0f�i��j�œ����������Ȃ�t�ɂ���
    public float maxForce = 10f;
    [Range(0, 10f)]
    public float testForce = 0f;    // Inspector �Œl�������邽�߂̕ϐ�

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // �����ł� testForce �� 0�`maxForce �ŕω�����
        // 0 �� �L�т���ԁAmaxForce �� ���S�ɋȂ��������
        float t = Mathf.Clamp01(testForce / maxForce);
        anim.SetFloat(paramName, t);
    }
}
