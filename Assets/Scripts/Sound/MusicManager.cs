using UnityEngine;

public class MusicManager : MonoBehaviour {
    // Müziði ve o müziðe özel ses seviyesini tutan yapý
    [System.Serializable]
    private struct MusicTrack {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MusicTrack[] musicTracks; // AudioClip[] yerine bunu kullanýyoruz

    [SerializeField] private Base baseObject;

    private bool isGameOver;

    void Start() {
        if (baseObject != null) {
            baseObject.OnDied += BaseObject_OnDied;
        }

        PlayRandomMusic();
    }

    private void BaseObject_OnDied() {
        audioSource.Stop();
        isGameOver = true;
    }

    private void Update() {
        if (!audioSource.isPlaying && !isGameOver) {
            PlayRandomMusic();
        }
    }

    private void PlayRandomMusic() {
        if (musicTracks == null || musicTracks.Length == 0) return;

        // musicTracks.Length kullanarak dinamik hale getirdik
        int randomIndex = Random.Range(0, musicTracks.Length);

        MusicTrack selectedTrack = musicTracks[randomIndex];

        // Seçilen müziðin kendi ses seviyesini atayýp çalýyoruz
        audioSource.clip = selectedTrack.clip;
        audioSource.volume = selectedTrack.volume;
        audioSource.Play();
    }

    private void OnDestroy() {
        // Event aboneliðini temizlemek bellek sýzýntýlarýný önler
        if (baseObject != null) {
            baseObject.OnDied -= BaseObject_OnDied;
        }
    }
}