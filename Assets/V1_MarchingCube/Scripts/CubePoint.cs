using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V1
{
    public class CubePoint : MonoBehaviour
    {
        public delegate void OnToggle();
        public OnToggle OnToggleAction;

        [SerializeField] private Color _goalColor;

        public int id;

        [SerializeField] private float _noiseValue;
        public float NoiseValue
        {
            get { return _noiseValue; }
            set
            {
                _noiseValue = value;
                float correctedNoiseValue = (_noiseValue + 1.0f) / 2.0f;
                _goalColor = new Color(correctedNoiseValue, correctedNoiseValue, correctedNoiseValue, correctedNoiseValue);
                GetComponent<MeshRenderer>().material.color = _goalColor;
                GetComponent<MeshRenderer>().sharedMaterial.color = _goalColor;
            }
        }

        private MeshRenderer _meshRenderer;

        void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        //private void OnMouseOver()
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        TogglePoint();
        //    }
        //}

        //private void TogglePoint()
        //{
        //    switch (_pointState)
        //    {
        //        case PointState.Inactive:
        //            _pointState = PointState.Active;
        //            break;
        //        case PointState.Active:
        //            _pointState = PointState.Inactive;
        //            break;
        //    }

        //    ChangeMaterial(_pointState);
        //    if (OnToggleAction != null) OnToggleAction();
        //}
    }
}