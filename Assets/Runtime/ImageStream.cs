using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Victeam.AIAssistant
{
    public class ImageStream : MonoBehaviour
    {
        [SerializeField] private float time = 1f;
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
            StartCoroutine(DownloadImage());
        }

        private static IEnumerator DownloadImage()
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
            yield return null;
        }
    }
}
