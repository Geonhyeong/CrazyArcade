using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class WaveController : MonoBehaviour
{
    public Vector3Int CellPos { get; set; } = Vector3Int.zero;
    public GameObject Host { get; set; }
    public int power = 0;
    Animator _animator;

    WaveDir _dir = WaveDir.None;
    public WaveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
                return;

            _dir = value;
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.26f, 0.26f);
        transform.position = pos;
        UpdateAnimation();

        GameObject.Destroy(gameObject, 0.5f);

    }

    void UpdateAnimation()
    {
        switch(_dir)
        {
            case WaveDir.Up:
                _animator.Play(power == 1 ? "UP_EDGE" : "UP");
                break;
            case WaveDir.Down:
                _animator.Play(power == 1 ? "DOWN_EDGE" : "DOWN");
                break;
            case WaveDir.Left:
                _animator.Play(power == 1 ? "LEFT_EDGE" : "LEFT");
                break;
            case WaveDir.Right:
                _animator.Play(power == 1 ? "RIGHT_EDGE" : "RIGHT");
                break;
        }
    }
}
