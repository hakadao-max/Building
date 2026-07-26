using UnityEngine;

namespace Test
{
    public class MTLightController : MonoBehaviour
    {
        public KeyCode key = KeyCode.F;
        public Light light;

        void Start()
        {
            light.enabled = false;
        }
        
        void Update()
        {
            if (key != KeyCode.None && RuntimeInput.GetKeyDown(key))
            {
                SwitchLight();
            }
        }

        private void SwitchLight()
        {
            light.enabled = !light.enabled;
        }


        public void TickLightPitch(float cameraControllerPitch)
        {
            transform.localRotation = Quaternion.Euler(cameraControllerPitch,0, 0);
        }
    }
}