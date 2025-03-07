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
        
        private OpenAIClient client;
        private List<Message> currentConversation = new List<Message>();
        private readonly string initialPrompt;

        public OpenAICommunicator(string initialPrompt)
        {
            client = new OpenAIClient(apiKey);
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
        
        public async Task<string> SendPrompt(string prompt, ImageUrl promptImageUrl)
        {
            // Exemple of ImageUrl: new ImageUrl(
            // "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Gfp-wisconsin-madison-the-nature-boardwalk.jpg/2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg",
            // ImageDetail.Low)
            currentConversation.Add(new Message(Role.User, new List<Content>()
            {
                prompt,
                promptImageUrl
            }));
            var chatRequest = new ChatRequest(currentConversation, Model.GPT4_Turbo);
            var response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);
            var choice = response.FirstChoice;
            
            Debug.Log((string)choice.Message);
            return choice.Message;
        }
    }
}
