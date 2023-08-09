using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GUI.GameWindow {
    public class DragPiece : MonoBehaviour {
        private Transform _pieceToDrag;

        private void OnMouseDown() {
            _pieceToDrag = transform.childCount != 0 ? transform.GetChild(0) : null;
            MoveToMouse();
        }

        private void OnMouseDrag() {
            MoveToMouse();
        }

        private void OnMouseUp() {
            MoveToNearestSquare();
        }

        private void MoveToMouse() {
            if (_pieceToDrag == null) {
                return;
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - Camera.main!.transform.position.z; // distance from camera
            _pieceToDrag.position = Camera.main!.ScreenToWorldPoint(mousePos);
        }

        private void MoveToNearestSquare() {
            if (_pieceToDrag == null) {
                return;
            }
            
            Vector2 mousePosWorld = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
            Collider2D pointCollider = Physics2D.OverlapPoint(mousePosWorld);
            
            if (pointCollider != null) {
                _pieceToDrag.position = pointCollider.transform.position;
                _pieceToDrag.parent = pointCollider.transform;
            }
            else {
                // return to original square
                _pieceToDrag.position = transform.position;
            }
        }
    }
}