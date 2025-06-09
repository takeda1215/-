using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ButtonView : MonoBehaviour
{
    public GameObject fingerObject;
    public AudioSource audioSource;
    public SkinnedMeshRenderer handRenderer;
    public string blendShapeName = "IndexTipPress";
    [Range(0f, 1f)] public float blendMaxRatio = 1f; // disp 演算上の最大比率

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
        // maxDisp は負の最大沈み込み量（例：-0.0013）
        // disp は 0（未押下） から maxDisp（最大押下：負）までの範囲

        // ──────────────── ここを変更 ────────────────
        // 旧: float normalized = Mathf.InverseLerp(maxDisp, 0f, disp);
        // 新: disp==0 → normalized=0, disp==maxDisp → normalized=1
        float normalized = Mathf.InverseLerp(0f, maxDisp, disp);
        // ────────────────────────────────────────────

        float weight = Mathf.Clamp01(normalized * blendMaxRatio) * 100f;
        handRenderer.SetBlendShapeWeight(bsIndex, weight);

        Debug.Log($"disp={disp:0.000000}, normalized={normalized:0.00}, weight={weight:0.00}");
    }


}
