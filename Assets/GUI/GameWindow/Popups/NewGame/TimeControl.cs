using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow.Popups.NewGame {
    public class TimeControl : MonoBehaviour {
        private enum DropdownValue { Unlimited, RealTime }
        
        [SerializeField] private Clocks _clocks;
        [SerializeField] private TMP_Dropdown _dropdown;
        [SerializeField] private Slider _minutesSlider;
        [SerializeField] private Slider _incrementSlider;
        [SerializeField] private TextMeshProUGUI _minutesValue;
        [SerializeField] private TextMeshProUGUI _incrementValue;
        private readonly float[] _sliderToMinutes = new float[38];
        private readonly float[] _sliderToIncrements = new float[31];

        private void Awake() {
            PopulateSliderToMinutes();
            PopulateSliderToIncrements();
            _minutesSlider.onValueChanged.AddListener(delegate { UpdateMinutesValue(); });
            _incrementSlider.onValueChanged.AddListener(delegate { UpdateIncrementValue(); });
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void PopulateRange(float[] array, int start, int stop, float step) {
            for (int i = start; i < stop; i++) {
                array[i] = array[i - 1] + step;
            }
        }

        private void PopulateSliderToMinutes() {
            _sliderToMinutes[0] = 0.25f;
            PopulateRange(_sliderToMinutes, 1, 4, 0.25f);
            PopulateRange(_sliderToMinutes, 4, 6, 0.5f);
            PopulateRange(_sliderToMinutes, 6, 24, 1);
            PopulateRange(_sliderToMinutes, 24, 29, 5);
            PopulateRange(_sliderToMinutes, 29, 38, 15);
        }

        private void PopulateSliderToIncrements() {
            _sliderToIncrements[0] = 0;
            PopulateRange(_sliderToIncrements, 1, 21, 1);
            PopulateRange(_sliderToIncrements, 21, 26, 5);
            PopulateRange(_sliderToIncrements, 26, 27, 15);
            PopulateRange(_sliderToIncrements, 27, 31, 30);
        }

        private void UpdateMinutesValue() {
            _clocks.startingMinutes = _sliderToMinutes[(int)_minutesSlider.value];
            _minutesValue.text = _clocks.startingMinutes.ToString(CultureInfo.CurrentCulture);
        }

        private void UpdateIncrementValue() {
            _clocks.incrementSeconds = _sliderToIncrements[(int)_incrementSlider.value];
            _incrementValue.text = _clocks.incrementSeconds.ToString(CultureInfo.CurrentCulture);
        }
        
        private void OnDropdownValueChanged(int value) {
            switch ((DropdownValue)value) {
                case DropdownValue.Unlimited:
                    _minutesSlider.gameObject.SetActive(false);
                    _incrementSlider.gameObject.SetActive(false);
                    _clocks.startingMinutes = Single.PositiveInfinity;
                    transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 375);
                    break;
                
                case DropdownValue.RealTime:
                    _minutesSlider.gameObject.SetActive(true);
                    _incrementSlider.gameObject.SetActive(true);
                    _clocks.startingMinutes = _sliderToMinutes[(int)_minutesSlider.value];
                    _clocks.incrementSeconds = _sliderToIncrements[(int)_incrementSlider.value];
                    transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 535);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}