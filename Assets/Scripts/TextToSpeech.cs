using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField]  public AudioSource audioSource;
    // Start is called before the first frame update
    async void Start()
    {
        var credentials = new BasicAWSCredentials(accessKey: "AKIAVY6OVFX3NGVFWM77", secretKey: "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");
        var client = new AmazonPollyClient(credentials,RegionEndpoint.USEast1);
        var request = new SynthesizeSpeechRequest()
        {
            Text = "Hello I'm Sherif",
            Engine= Engine.Generative,
            VoiceId= VoiceId.Matthew,
            OutputFormat=OutputFormat.Mp3
        };
        var response= await client.SynthesizeSpeechAsync(request);
        WriteIntoFile(response.AudioStream);
        using (var www = UnityWebRequestMultimedia.GetAudioClip($"{Application.persistentDataPath}/audio.mp3",AudioType.MPEG))
        {
            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();
            var clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }

    }
    private void WriteIntoFile(Stream stream)
    {
        using (var filestream = new FileStream(path:$"{Application.persistentDataPath}/audio.mp3",FileMode.Create))
        {
            byte[] buffer = new byte[8*1024];
            int bytesRead=0;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length))>0)
            {
                filestream.Write(buffer, 0, bytesRead);
            }
        }
    }
}


