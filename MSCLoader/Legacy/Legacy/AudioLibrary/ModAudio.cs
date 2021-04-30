using AudioLibrary;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    public class ModAudio : MonoBehaviour
    {
        public AudioSource audioSource;

        public void LoadAudioFromFile(string path, bool doStream, bool background)
        {
            try
            {
                AudioFormat format = Manager.GetAudioFormat(path);
                if (format == AudioFormat.unknown) format = AudioFormat.mp3;

                if (audioSource == null) audioSource = gameObject.GetComponent<AudioSource>();
                audioSource.clip = Manager.Load(new MemoryStream(File.ReadAllBytes(path)), format, Path.GetFileName(path), doStream, background, true);
            }
            catch (Exception e)
            {
                ModConsole.LogError(e.Message);
                System.Console.WriteLine(e);
                audioSource.clip = null;
            }
        }

        public TimeSpan Time() => TimeSpan.FromSeconds(audioSource.clip != null ? audioSource.time : 0);

        public void Play()
        {
            audioSource.mute = false;
            audioSource.Play();
        }

        public void Play(float time, float delay = 1f) => StartCoroutine(PlayDelayed(time, delay));

        IEnumerator PlayDelayed(float time, float delay)
        {
            yield return new WaitForSeconds(delay);
            audioSource.mute = false;
            audioSource.time = time;
            audioSource.Play();
        }

        public void Stop() => audioSource.Stop();
    }
}