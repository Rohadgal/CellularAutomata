using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool _isWhite = true;
    SpriteRenderer _spriteRenderer;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(_isWhite) {
            _spriteRenderer.color = Color.white;
        }
    }

    public void setCellColor(bool isWhite) {
        _isWhite = isWhite;
        if(_spriteRenderer != null) {
           if(!_isWhite) {
             _spriteRenderer.color = Color.red;
             return;
           }
           _spriteRenderer.color = Color.white;
        }
    }

    public Color getCellColor() {
        return _spriteRenderer.color;
    }

    public bool getColor() { return _isWhite; }
}