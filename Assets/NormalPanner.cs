using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVPan : MonoBehaviour
{
    [SerializeField]
    private float _speed = 0.05f;

    private Renderer _renderer;
    
    private void Awake() => _renderer = GetComponent<Renderer>();

    void Update() => _renderer.sharedMaterial.mainTextureOffset += new Vector2(_speed * Time.deltaTime, _speed * Time.deltaTime);
}
