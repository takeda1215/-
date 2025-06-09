using UnityEngine;

public class BlendShapeTester : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer smr;
    [SerializeField] string shapeName = "IndexTipPress";

    int idx;

    void Start()
    {
        idx = smr.sharedMesh.GetBlendShapeIndex(shapeName);
        if (idx < 0) Debug.LogError($"BlendShape '{shapeName}' が見つかりません");
    }

    void Update()
    {
        // 0〜100をPingPongでループ
        float weight = Mathf.PingPong(Time.time * 50f, 100f);
        smr.SetBlendShapeWeight(idx, weight);
    }
}
