using UnityEngine;
using UnityEditor;
using System.IO;

public class AudioTrimmer : EditorWindow
{
    [MenuItem("Tools/Trim Audio Silence")]
    public static void TrimAudio()
    {
        AudioClip clip = Selection.activeObject as AudioClip;
        if (clip == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select an AudioClip in the Project window.", "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(clip);
        if (string.IsNullOrEmpty(path)) return;

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // Find the first sample that is louder than a small threshold
        float threshold = 0.01f;
        int startSample = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) > threshold)
            {
                // Go back a little bit so it doesn't clip abruptly (e.g., 0.05 seconds)
                startSample = Mathf.Max(0, i - (int)(clip.frequency * clip.channels * 0.05f));
                break;
            }
        }

        // Find the end sample
        int endSample = samples.Length - 1;
        for (int i = samples.Length - 1; i >= 0; i--)
        {
            if (Mathf.Abs(samples[i]) > threshold)
            {
                endSample = Mathf.Min(samples.Length - 1, i + (int)(clip.frequency * clip.channels * 0.05f));
                break;
            }
        }

        if (startSample >= endSample)
        {
            EditorUtility.DisplayDialog("Error", "Audio is completely silent or too quiet.", "OK");
            return;
        }

        int newSampleCount = endSample - startSample;
        float[] trimmedSamples = new float[newSampleCount];
        System.Array.Copy(samples, startSample, trimmedSamples, 0, newSampleCount);

        string directory = Path.GetDirectoryName(path);
        string filename = Path.GetFileNameWithoutExtension(path) + "_Trimmed.wav";
        string newPath = Path.Combine(directory, filename);

        SaveWav(newPath, trimmedSamples, clip.channels, clip.frequency);
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Trimmed audio saved to:\n" + newPath, "OK");
    }

    private static void SaveWav(string path, float[] samples, int channels, int sampleRate)
    {
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                int sampleCount = samples.Length;
                int byteRate = sampleRate * channels * 2;
                int blockAlign = channels * 2;

                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + sampleCount * 2);
                bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((short)1); // PCM
                bw.Write((short)channels);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write((short)blockAlign);
                bw.Write((short)16); // bits per sample
                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(sampleCount * 2);

                int max = short.MaxValue;
                for (int i = 0; i < samples.Length; i++)
                {
                    bw.Write((short)(samples[i] * max));
                }
            }
        }
    }
}
