using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Victeam.AIAssistant
{
    public class ImageStream : MonoBehaviour
    {
        [SerializeField] private float time = 5f;
        private float timeLeft;

        private void Start()
        {
            timeLeft = time;
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft > 0) return;

            timeLeft = time;
            CaptureImage();
        }

        private static async Task CaptureImage()
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
            byte[] fileData = File.ReadAllBytes("Screenshot.png");
            var tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
			await OpenAICommunicator.Instance.SendPrompt("Describe this image to me", tex);
        }
    }
}
