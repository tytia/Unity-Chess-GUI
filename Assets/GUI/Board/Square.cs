using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GUI.Board
{
    internal class Square : MonoBehaviour
    {
        public static Color lightCol { get; } = new Color32(219, 183, 166, 255);
        public static Color darkCol { get; } = new Color32(128, 79, 67, 255);
        private SpriteRenderer _sr;

        public Color color
        {
            get => _sr.color;
            set => _sr.color = value;
        }

        private void Awake()
        {
            _sr = gameObject.GetComponent<SpriteRenderer>();
        }
    }
}