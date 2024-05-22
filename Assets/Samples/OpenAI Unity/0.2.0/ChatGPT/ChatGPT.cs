using UnityEngine;
using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private string openaiApiKey;
        private string awsPollyAccessKey;
        private string awsPollySecretKey;


        private OpenAIApi openai;
        public Text ReplyText;
        public GameObject[] statues; // Array of statue game objects
        public GameObject targetGameObject; // The game object to monitor
        private string prompt = "this is an ar museum project with unity.Act as a Guide in Museum and only reply to the questions of the visitor who talks to you.\n" +
            "don't ask questions.\n" +
            "reply to this message with greeting me (the visitor) and ask me if I want any help in the museum.\n" +
            " Don't break character.\n" +
            " Don't ever mention that you are an AI model even if you didn't find an answer to the questions.\n" +
            "There are statues that exist in the museum which I might ask you about.\n" +
            "these are the available staues in the museum :'Attalus Statue', 'Beethoven Statue', 'Gir Statue', 'Monkey Statue', 'Moai Statue', 'Kingdom Sphinx Statue', 'Amenhotep III Statue', 'Horus Statue', 'Egyptian Dog Statue', 'Nefertiti Statue', 'Anubis Statue'.\n" +
            "if the vistor asks to show one of these statue reply with 'sure, here is the statue of ...' and reply with the requested statue name\n" +
            "if the vistor asks about a statue that does not exist in the museum reply with 'the museum will be upgraded soon and we will add that ...' and mention the statue the user asked\n" +
            "i'm using whisper in unity to convert the speech to text so consider that the visitor may not say the statue name correctly\n";
        private bool isfirst = true;
        private bool isInitialized = false; // Flag to check if initial prompt has been sent
        private readonly List<string> availableStatues = new List<string> { "Attalus Statue", "Beethoven Statue", "Gir Statue", "Monkey Statue", "Moai Statue", "Kingdom Sphinx Statue", "Amenhotep III Statue", "Horus Statue", "Egyptian Dog Statue", "Nefertiti Statue", "Anubis Statue" };
        private List<ChatMessage> conversationHistory = new List<ChatMessage>(); // Shared conversation history

        private void Start()
        {
            // Load environment variables
            string envFilePath = Path.Combine(Application.dataPath, ".env");
            var envVars = EnvReader.LoadEnv(envFilePath);

            // Get API keys from environment variables
            envVars.TryGetValue("OPENAI_API_KEY", out openaiApiKey);
            envVars.TryGetValue("AWS_POLLY_ACCESS_KEY", out awsPollyAccessKey);
            envVars.TryGetValue("AWS_POLLY_SECRET_KEY", out awsPollySecretKey);

            // Initialize OpenAI API
            openai = new OpenAIApi(openaiApiKey);
        }

        private void Update()
        {
            // Check if the target game object is active and the initial prompt has not been sent
            if (targetGameObject.activeSelf && !isInitialized)
            {
                isInitialized = true; // Set the flag to prevent re-initialization
                SendInitialPrompt(); // Send the initial prompt
            }
        }

        private async void SendInitialPrompt()
        {
            // Send the initial prompt
            var initialMessage = new ChatMessage()
            {
                Role = "user",
                Content = prompt
            };

            // Add initial message to conversation history
            conversationHistory.Add(initialMessage);

            // Synthesize speech from the initial prompt
            await PlayVoiceResponse(prompt);
        }

        public async Task<string> GetVoiceResponse(string text)
        {
            var userMessage = new ChatMessage() { Role = "user", Content = text };
            conversationHistory.Add(userMessage);

            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = conversationHistory
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                var responseText = message.Content.Trim();
                ReplyText.text = responseText;

                // Add response message to conversation history
                conversationHistory.Add(message);

                if (!isfirst)
                {
                    // Normalize the text for matching
                    string normalizedText = text.ToLower().Trim();
                    normalizedText = Regex.Replace(normalizedText, @"\s+", " "); // Remove extra spaces

                    // Check if the user requested a statue
                    foreach (var statue in availableStatues)
                    {
                        string statueLower = statue.ToLower();

                        // Simplified matching logic for various ways of requesting a statue
                        if (normalizedText.Contains(statueLower))
                        {
                            ActivateStatue(statue);

                            // Generate the custom response for the statue
                            responseText = await GetCustomStatueResponse(statue);
                            break; // Exit the loop after processing the first matching statue
                        }
                    }
                }

                isfirst = false; // Set isfirst to false after the initial prompt
                return responseText; // Return the generated response
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
                return "";
            }
        }

        public async Task<string> GetCustomStatueResponse(string statue)
        {
            // Generate the custom prompt
            string customPrompt = $"The user is looking at the {statue} statue. Generate a short description or interesting facts about it and start your reply with 'this beautiful piece you are looking at:' then continue with the description.";

            var userMessage = new ChatMessage() { Role = "user", Content = customPrompt };
            conversationHistory.Add(userMessage);

            var customResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = conversationHistory
            });

            if (customResponse.Choices != null && customResponse.Choices.Count > 0)
            {
                var customMessage = customResponse.Choices[0].Message;
                var responseText = customMessage.Content.Trim();
                ReplyText.text = responseText;

                // Add custom response message to conversation history
                conversationHistory.Add(customMessage);

                return responseText; // Return the custom generated response
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
                return "";
            }
        }

        public async Task PlayVoiceResponse(string text)
        {
            string response = await GetVoiceResponse(text);
            if (!string.IsNullOrEmpty(response))
            {
                var credentials = new BasicAWSCredentials(awsPollyAccessKey, awsPollySecretKey);
                var client = new AmazonPollyClient(credentials, RegionEndpoint.USEast1);

                var request = new SynthesizeSpeechRequest()
                {
                    Text = response,
                    Engine = Engine.Neural,
                    VoiceId = VoiceId.Matthew,
                    OutputFormat = OutputFormat.Mp3
                };

                var audioStream = (await client.SynthesizeSpeechAsync(request)).AudioStream;

                PlayAudioClipFromStream(audioStream);
            }
        }

        private async void PlayAudioClipFromStream(Stream audioStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await audioStream.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();

                // Write audio data to a temporary file
                string filePath = Path.Combine(Application.temporaryCachePath, "audio.mp3");
                File.WriteAllBytes(filePath, audioBytes);

                // Load and play audio from the temporary file
                StartCoroutine(LoadAudioClipAndPlay(filePath));
            }
        }

        private IEnumerator LoadAudioClipAndPlay(string filePath)
        {
            using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
            {
                var op = www.SendWebRequest();
                while (!op.isDone)
                    yield return null;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogError($"Failed to load audio clip: {www.error}");
                }
            }
        }

        private void ActivateStatue(string statueName)
        {
            foreach (var statue in statues)
            {
                if (statue.name.ToLower() == statueName.ToLower())
                {
                    statue.SetActive(true); // Activate the statue game object
                }
            }
        }
    }
}
