using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ButtonView : MonoBehaviour
{
    public GameObject fingerObject;
    public AudioSource audioSource;
    public SkinnedMeshRenderer handRenderer;
    public string blendShapeName = "IndexTipPress";
    [Range(0f, 1f)] public float blendMaxRatio = 1f; // disp ���Z��̍ő�䗦

    int bsIndex;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        bsIndex = handRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
    }

    public void Initialize(ButtonModel model, float maxDisp)
    {
        model.OnFingerShow += ShowFinger;
        model.OnFingerHide += HideFinger;
        model.OnPressed += PlaySound;
        model.OnDisplacementChanged += disp => DriveBlendShape(disp, maxDisp);
    }

    void ShowFinger() => fingerObject.SetActive(true);
    void HideFinger() => fingerObject.SetActive(false);
    void PlaySound() => audioSource.Play();

    void DriveBlendShape(float disp, float maxDisp)
    {
        // maxDisp �͕��̍ő咾�ݍ��ݗʁi��F-0.0013�j
        // disp �� 0�i�������j ���� maxDisp�i�ő剟���F���j�܂ł͈̔�

        // �������������������������������� ������ύX ��������������������������������
        // ��: float normalized = Mathf.InverseLerp(maxDisp, 0f, disp);
        // �V: disp==0 �� normalized=0, disp==maxDisp �� normalized=1
        float normalized = Mathf.InverseLerp(0f, maxDisp, disp);
        // ����������������������������������������������������������������������������������������

        float weight = Mathf.Clamp01(normalized * blendMaxRatio) * 100f;
        handRenderer.SetBlendShapeWeight(bsIndex, weight);

        Debug.Log($"disp={disp:0.000000}, normalized={normalized:0.00}, weight={weight:0.00}");
    }


}
