using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float speed = 10f;
	public Vector3 direction;
	public float lifetime = 1f;
    public AudioClip cannonSound; // Reference to your cannon_01.wav file

	// Use this for initialization
	void Start () {
        // Play cannon sound when bullet is created
        PlayCannonSound();
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += direction * speed * Time.deltaTime;

		lifetime -= Time.deltaTime;
		if (lifetime <= 0f) {
			Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter (Collider collider) {
		if (collider.GetComponent<Enemy> () != null) {
			Destroy (collider.gameObject);
			Destroy (this.gameObject);
		}
	}

    void PlayCannonSound() {
        // Method 1: Play sound at bullet position (3D sound)
        if (cannonSound != null) {
            AudioSource.PlayClipAtPoint(cannonSound, transform.position);
        }
    }
}
