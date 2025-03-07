using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using UnityEngine;

namespace Victeam.AIAssistant
{
    public class OpenAICommunicator
    {
        private static readonly string apiKey = "";
        private static readonly string initialPrompt = @"You are a prototype for an AI assistant package in the Unity Game Engine.
While you may get more specific tasks, your purpose for now is to provide general helpful feedback to the user.
You will get multiple input formats, including just text, but also images and audio.

Since you are a high-end assistant, hallucinating facts is unacceptable. Be absolutely sure of everything you say, and when you are unable to answer,
make sure to tell the user.
Good luck!";
        
        // ----- SINGLETON PATTERN -----
        private static OpenAICommunicator instance;

        public static OpenAICommunicator Instance
        {
            get
            {
                if (instance == null)
                {
                    var api = new OpenAIClient(apiKey);
                    instance = new OpenAICommunicator(api);
                }

                return instance;
            }
        }
        
        // -----------------------------

        private OpenAIClient client;
        private List<Message> currentConversation = new List<Message>();

        OpenAICommunicator(OpenAIClient client)
        {
            this.client = client;
            currentConversation.Add(new Message(Role.System, initialPrompt));
        }

        public async Task<string> SendPrompt(string prompt)
        {
            currentConversation.Add(new Message(Role.User, prompt));
            var chatRequest = new ChatRequest(currentConversation, Model.GPT4_Turbo);
            var response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);
            var choice = response.FirstChoice;
            string responseMessage = choice.Message;
            currentConversation.Add(new Message(Role.Assistant, responseMessage));
            
            return responseMessage;
        }
        
        public async Task<string> SendPrompt(string prompt, Texture2D promptImage)
        {
            currentConversation.Add(new Message(Role.User, new List<Content>()
            {
                prompt,
                promptImage
            }));
            var chatRequest = new ChatRequest(currentConversation, Model.GPT4_Turbo);
            var response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);
            var choice = response.FirstChoice;
            
            Debug.Log((string)choice.Message);
            return choice.Message;
        }
    }
}
