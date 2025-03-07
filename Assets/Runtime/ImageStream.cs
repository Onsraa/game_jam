using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            // StartCoroutine(CaptureImage());
        }

        private static IEnumerator CaptureImage(Action<Texture2D> callback)
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
            byte[] fileData = File.ReadAllBytes("Screenshot.png");
            var tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            callback(tex);
            yield return null;
        }
    }
}
