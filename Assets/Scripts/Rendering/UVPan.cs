using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVPan : MonoBehaviour
{
    private const float _padDivisor = 100.0f;

    [SerializeField]
    private float _speedX = 1.0f;
    
    [SerializeField]
    private float _speedY = 1.0f;

    private Renderer _renderer;
    private void Awake() => _renderer = GetComponent<Renderer>();
    private void Update()
        => _renderer.material.mainTextureOffset += new Vector2(_speedX, _speedY) / _padDivisor * Time.deltaTime;
}

