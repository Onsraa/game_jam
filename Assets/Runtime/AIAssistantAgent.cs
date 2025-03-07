using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Victeam.AIAssistant
{
    public class AIAssistantAgent : MonoBehaviour
    {
        [TextArea(15, 20)]
        [SerializeField] private string initialPrompt;

        public OpenAICommunicator Communicator;
        
        private void Start()
        {
            Communicator = new OpenAICommunicator(initialPrompt);
        }
    }
}
