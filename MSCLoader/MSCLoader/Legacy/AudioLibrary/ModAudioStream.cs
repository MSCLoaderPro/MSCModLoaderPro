using AudioLibrary.MP3_Streaming;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    public class ModAudioStream : MonoBehaviour
    {
        public AudioSource audioSource;
        public string songInfo;
        public bool showDebug =false;

        string bufferInfo;
        bool showDebugInfo, done = false;
        MP3Stream mp3s = new MP3Stream();

        public void PlayStream(string streamURL)
        {
            mp3s.audioSource = audioSource;
            mp3s.PlayStream(streamURL);

            showDebugInfo = showDebug;
        }

        public void StopStream()
        {
            mp3s.Dispose();
            done = false;
            audioSource.clip = null;
        }

        void Awake() => audioSource = gameObject.GetComponent<AudioSource>();

        void FixedUpdate()
        {
            mp3s.UpdateLoop();
            bufferInfo = mp3s.buffer_info;
            songInfo = mp3s.song_info;
        }

        void OnGUI()
        {
            if (showDebugInfo)
            {
                GUI.Label(new Rect(1, Screen.height - 22, Screen.width, 22), string.Format("<color=orange>{0}</color> | Buffer: <color=orange>{1}</color> | Metadata: <color=orange>{2}</color>{3}", mp3s.playbackState.ToString(), bufferInfo, songInfo, mp3s.IsBufferNearlyFull ? " | <color=red>Buffer full</color>" : ""));
                showDebugInfo = showDebug;
            }
        }

        void Update()
        {
            if (mp3s.decomp && !done)
            {
                audioSource.clip = AudioClip.Create("mp3_Stream", int.MaxValue,
                    mp3s.bufferedWaveProvider.WaveFormat.Channels,
                    mp3s.bufferedWaveProvider.WaveFormat.SampleRate,
                    true, new AudioClip.PCMReaderCallback(mp3s.ReadData)
                );

                done = true;
            }
        }

        void OnApplicationQuit() => mp3s.Dispose();
    }
}