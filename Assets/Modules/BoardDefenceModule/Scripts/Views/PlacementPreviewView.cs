using UnityEngine;

namespace BoardDefence.Views
{
    public class PlacementPreviewView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.5f);

        private Material _material;
        private bool _isValid;

        private void Awake()
        {
            if (_meshRenderer == null)
                _meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (_meshRenderer != null)
            {
                if (_meshRenderer.sharedMaterial != null)
                {
                    _material = new Material(_meshRenderer.sharedMaterial);
                }
                else
                {
                    _material = new Material(Shader.Find("Standard"));
                    _material.SetFloat("_Mode", 3);
                    _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    _material.SetInt("_ZWrite", 0);
                    _material.EnableKeyword("_ALPHABLEND_ON");
                    _material.renderQueue = 3000;
                }
                _meshRenderer.material = _material;
            }

            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetValid(bool isValid)
        {
            _isValid = isValid;

            if (_material != null)
            {
                _material.color = isValid ? _validColor : _invalidColor;
            }
        }

        public void SetDefenceType(string defenceKey)
        {
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }
    }
}
