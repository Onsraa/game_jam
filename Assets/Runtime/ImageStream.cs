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
        [SerializeField] private AudioSource audioSourceGameobject;

        private float timeLeft;
        private AudioSource audioSource;

        private void Start()
        {
            timeLeft = time;
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft > 0) return;

            timeLeft = time;
            CaptureImage();
        }

        private async Task CaptureImage()
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
            byte[] fileData = File.ReadAllBytes("Screenshot.png");
            var tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
			var resultText = await OpenAICommunicator.Instance.SendPrompt("Describe this image to me", tex);
            var resultAudioClip = await OpenAICommunicator.Instance.SendTextToSpeech(resultText);
            audioSource.PlayOneShot(resultAudioClip);
        }
    }
}
