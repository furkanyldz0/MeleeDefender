using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider fxSlider;
    [SerializeField] private Slider musicSlider;

    private void Start() {
        if (masterSlider != null)
            masterSlider.value = PlayerPrefs.GetFloat("masterVolumePref", 0.6f);

        if (fxSlider != null)
            fxSlider.value = PlayerPrefs.GetFloat("soundFXVolumePref", 1f);

        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("musicVolumePref", 1f);
    }
}
