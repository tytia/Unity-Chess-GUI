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
            SnapToSquare();
        }

        private void MoveToMouse() {
            if (_pieceToDrag == null) {
                return;
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - Camera.main!.transform.position.z; // distance from camera
            _pieceToDrag.position = Camera.main!.ScreenToWorldPoint(mousePos);
        }

        private void SnapToSquare() {
            if (_pieceToDrag == null) {
                return;
            }
            
            Ray ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            if (hit.collider != null) {
                _pieceToDrag.position = hit.transform.position;
                _pieceToDrag.parent = hit.transform;
            }
            else {
                _pieceToDrag.position = transform.position;
            }
        }
    }
}