using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    private void Start() {
        SetMasterVolume(PlayerPrefs.GetFloat("masterVolumePref", 0.6f));
        SetSoundFXVolume(PlayerPrefs.GetFloat("soundFXVolumePref", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("musicVolumePref", 1f));
    }

    public void SetMasterVolume(float level) {
        //audioMixer.SetFloat("masterVolume", level);
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);

        PlayerPrefs.SetFloat("masterVolumePref", level);
        PlayerPrefs.Save();
    }

    public void SetSoundFXVolume(float level) {
        //audioMixer.SetFloat("soundFXVolume", level);
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20f);

        PlayerPrefs.SetFloat("soundFXVolumePref", level);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float level) {
        //audioMixer.SetFloat("musicVolume", level);
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);

        PlayerPrefs.SetFloat("musicVolumePref", level);
        PlayerPrefs.Save();
    }
}
