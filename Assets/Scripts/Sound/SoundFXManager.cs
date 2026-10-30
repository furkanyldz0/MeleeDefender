using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance { get; private set; }

    [SerializeField] private AudioSource soundFXObject;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Sahnede birden fazla SoundFXManager var!");
        }
        Instance = this;
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume, bool isPitched = false) {

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;
        audioSource.volume = volume;

        if (isPitched) {
            float randomPitch = Random.Range(9, 12) / 10f;
            audioSource.pitch = randomPitch;

            Debug.Log(randomPitch);
        }

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume, bool isPitched = false) {

        int rand = Random.Range(0, audioClip.Length);

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip[rand];
        audioSource.volume = volume;

        if (isPitched) {
            float randomPitch = Random.Range(9, 12) / 10f;
            audioSource.pitch = randomPitch;

            Debug.Log(randomPitch);
        }

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}
