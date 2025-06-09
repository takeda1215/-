using System.Collections.Generic;

/// <summary>
/// 移動平均フィルタ. 
/// 過去の値と現在の値の平均値を、新しい値とする.
/// </summary>
public class MovingAverageFilter
{
    private Queue<float> _queue;
    private int _queueSize;
    private float _queueSum;

    /// <summary>
    /// 移動平均フィルタで考慮するフレーム数. 
    /// 必要ない場合は1以下にする.
    /// </summary>
    public int QueueSize
    {
        set { _queueSize = value > 0 ? value : 1; }
    }

    /// <summary>
    /// 引数は移動平均を考慮するフレーム数.
    /// </summary>
    /// <param name="size">移動平均を考慮するフレーム数.</param>
    public MovingAverageFilter(int size)
    {
        _queue = new Queue<float>();
        _queueSize = size;
        _queueSum = 0;
    }

    /// <summary>
    /// 引数のデータを新しくフィルタに入れる. 
    /// </summary>
    /// <param name="newValue">新しく受信したデータ.</param>
    /// <returns>移動平均フィルタの適用結果.</returns>
    public float ApplyFilter(float newValue)
    {
        //キューに新しい値を追加.
        _queue.Enqueue(newValue);
        _queueSum += newValue;

        //キューのサイズを超えた場合、古い値を削除.
        AdjustQueueSize();

        //移動平均フィルタの適用結果.
        return _queueSum / _queue.Count;
    }

    /// <summary>
    /// フィルタサイズに合わせて"queueSum"を更新.
    /// </summary>
    private void AdjustQueueSize()
    {
        while (_queue.Count > _queueSize)
        {
            _queueSum -= _queue.Dequeue();
        }
    }
}
