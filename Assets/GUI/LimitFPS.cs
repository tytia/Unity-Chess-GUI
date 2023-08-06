using UnityEngine;

namespace GUI
{
    public class LimitFPS : MonoBehaviour
    {
        void Start()
        {
            Application.targetFrameRate = 60;
        }
    }
}
