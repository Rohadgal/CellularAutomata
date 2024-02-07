using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCell : MonoBehaviour
{
    [SerializeField]
    bool _isAlive = true;
    Material _material;
    MeshRenderer _meshRenderer;
    public int _state;

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

    public int getState() {
        return _state;
    }

    public void setCube(bool isAlive) {
        _isAlive = isAlive;
        // check if mesh renderer is null
        if( _meshRenderer != null ) {
            if(!_isAlive) {
                if(_state <= 0) {
                   // Debug.LogWarning("off");
                    _meshRenderer.enabled = false;
                    return;
                }
                _meshRenderer.material.color = Color.red;
                return;
            }
            _meshRenderer.enabled=true;
            _meshRenderer.material.color = Color.white;
        }
    }

    public void setState(int t_state) {
        _state = t_state;
    }
}
    