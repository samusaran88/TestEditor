// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;

namespace TriLibCore.Samples
{
    /// A renderer class that renders a point cloud contained by PointCloudData.
    [ExecuteInEditMode]
    public sealed class PointCloudRenderer : UnityEngine.MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] PointCloudData _sourceData = null;

        [SerializeField] private bool _destroyData;

        public PointCloudData sourceData {
            get { return _sourceData; }
            set { _sourceData = value; }
        }

        [SerializeField] Color _pointTint = new Color(0.5f, 0.5f, 0.5f, 1);

        public Color pointTint {
            get { return _pointTint; }
            set { _pointTint = value; }
        }

        [SerializeField] float _pointSize = 0.05f;

        public float pointSize {
            get { return _pointSize; }
            set { _pointSize = value; }
        }

        public bool destroyData
        {
            get
            {
                return _destroyData;
            }
            set
            {
                _destroyData = value;
            }
        }

        #endregion

        #region Public properties (nonserialized)

        public ComputeBuffer sourceBuffer { get; set; }

        #endregion

        #region Internal resources

        [SerializeField, HideInInspector] Shader _pointShader = null;
        [SerializeField, HideInInspector] Shader _diskShader = null;

        #endregion

        #region Private objects

        Material _pointMaterial;
        Material _diskMaterial;

        #endregion

        #region MonoBehaviour implementation

        void OnValidate()
        {
            _pointSize = Mathf.Max(0, _pointSize);
        }

        void OnDestroy()
        {
            if (_pointMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_pointMaterial);
                    Destroy(_diskMaterial);
                    if (_destroyData && sourceData != null)
                    {
                        Destroy(sourceData);
                    }
                }
                else
                {
                    DestroyImmediate(_pointMaterial);
                    DestroyImmediate(_diskMaterial);
                    if (_destroyData && sourceData != null)
                    {
                        DestroyImmediate(sourceData);
                    }
                }
            }
        }

        void OnRenderObject()
        {
            // We need a source data or an externally given buffer.
            if (_sourceData == null && sourceBuffer == null) return;

            // Check the camera condition.
            var camera = Camera.current;
            if ((camera.cullingMask & (1 << gameObject.layer)) == 0) return;
            if (camera.name == "Preview Scene Camera") return;

            // TODO: Do view frustum culling here.

            // Lazy initialization
            if (_pointMaterial == null)
            {
                _pointShader = Shader.Find("Point Cloud/Point");
                _pointMaterial = new Material(_pointShader);
                _pointMaterial.hideFlags = HideFlags.DontSave;
                _pointMaterial.EnableKeyword("_COMPUTE_BUFFER");

                _diskShader = Shader.Find("Point Cloud/Disk");
                _diskMaterial = new Material(_diskShader);
                _diskMaterial.hideFlags = HideFlags.DontSave;
                _diskMaterial.EnableKeyword("_COMPUTE_BUFFER");
            }

            // Use the external buffer if given any.
            var pointBuffer = sourceBuffer != null ?
                sourceBuffer : _sourceData.computeBuffer;

            if (_pointSize == 0)
            {
                _pointMaterial.SetPass(0);
                _pointMaterial.SetColor("_Tint", _pointTint);
                _pointMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                _pointMaterial.SetBuffer("_PointBuffer", pointBuffer);
                Graphics.DrawProceduralNow(MeshTopology.Points, pointBuffer.count, 1);
            }
            else
            {
                _diskMaterial.SetPass(0);
                _diskMaterial.SetColor("_Tint", _pointTint);
                _diskMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                _diskMaterial.SetBuffer("_PointBuffer", pointBuffer);
                _diskMaterial.SetFloat("_PointSize", pointSize);
                Graphics.DrawProceduralNow(MeshTopology.Points, pointBuffer.count, 1);
            }
        }

        #endregion
    }
}
