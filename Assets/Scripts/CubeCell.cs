using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCell : MonoBehaviour
{
    [SerializeField]
    bool _isAlive = true;
    Material _material;
    MeshRenderer _meshRenderer;

    private void Awake() {
        if(_meshRenderer == null) {
            _meshRenderer = new MeshRenderer();
        }
        if (_material == null) {
            _material = new Material(Shader.Find("Standard"));
            _material.color = new Color(1, 1, 1, 1); // Set material to white color
        }
        _meshRenderer = GetComponent<MeshRenderer>();
        _material = GetComponent<Renderer>().material;
        if(_isAlive ) {
            _material.color = Color.white;
            
        }
    }

    //public void setCubeColor(bool isAlive) {
    //    _isAlive = isAlive;
    //    if(_material != null ) {
    //        if(!_isAlive) {
    //            _material.color = Color.magenta;
    //            return;
    //        }
    //        _material.color = Color.white;
    //        return;
    //    }
    //    Debug.Log("Null material");
    //}

    public bool getIsAlive() {
        return _isAlive;
    }

    public void setCube(bool isAlive) {
        _isAlive = isAlive;
        if( _meshRenderer != null ) {
            if(!_isAlive) {
                _meshRenderer.enabled = false;
                return;
            }
            _meshRenderer.enabled=true;
        }
    }
}
    