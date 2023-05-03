using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrollsAndDungeons
{
	public class PlayerBehaviour : MonoBehaviour
	{
		private Rigidbody2D _rb;
		private float speed = 4f;
		private Vector2 MoveDirection;
		public int Health = 100;
		public float TorchPickupTime = 6f;
		public uint GemsCount;
		public TextMeshProUGUI GemText;
		public TextMeshProUGUI HPText;
		public Slider HealthSlider;
		public float LastHitTime;
		//Trolls will flee when torch is active (NOT troll chiefs)
		public float TorchActiveTime = 6.0f;
		public float LastTorchActivationTime;
		public bool HasTorch;
		public GameObject TorchSprite;
		public SpriteRenderer SpriteRenderer;
		public GameObject GameOverScreen;


		void Start()
		{
			_rb = GetComponent<Rigidbody2D>();
			SpriteRenderer = GetComponent<SpriteRenderer>();

        }
		private void Update()
		{
			GetInput();
			GemText.text = GemsCount.ToString();
            HPText.text = Health.ToString();
			HealthSlider.value = Health / 100f;
			if (Time.time - LastTorchActivationTime >= TorchActiveTime)
			{
                HasTorch = false;
				TorchSprite.SetActive(false);
            }

            if (Health <= 0)
			{
				Time.timeScale = 0f;
				GameOverScreen.SetActive(true);
				GeneratorManager.Instance.IsPaused = true;
			}

        }

        private void GetInput()
		{
            Vector2 MoveDir = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
                MoveDir += Vector2.up;
            if (Input.GetKey(KeyCode.S))
                MoveDir += Vector2.down;
            if (Input.GetKey(KeyCode.A))
                MoveDir += Vector2.left;
            if (Input.GetKey(KeyCode.D))
                MoveDir += Vector2.right;
			MoveDirection = MoveDir;
        }

		void FixedUpdate()
        {
            if (MoveDirection != Vector2.zero)
            {
                _rb.MovePosition(_rb.position + MoveDirection.normalized * speed * Time.fixedDeltaTime);
				MoveDirection = Vector2.zero;
				_rb.velocity = Vector2.zero;
            }
        }


		private void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.CompareTag("Gem"))
				GemsCount++;
			else if (collision.CompareTag("Trap"))
			{
                Health -= 5;
                StartCoroutine(TakeDamageEffect());
            }
			else if (collision.CompareTag("Torch") 
				&& Time.time - LastTorchActivationTime >= TorchActiveTime && TorchSprite.activeSelf == false)
			{
				TorchSprite.SetActive(true);
                HasTorch = true;
				LastTorchActivationTime = Time.time;
            }

			GeneratorManager.Instance.UseObject(collision.transform.position);
        }
		//To avoid spam attacks when cornered
		private bool CanBeHit()
		{
			if (Time.time - LastHitTime >= 2f)
				return true;
			else
				return false;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.CompareTag("Thief") && CanBeHit())
			{
				if (GemsCount < 2)
					GemsCount = 0;
				else
					GemsCount -= 2;
				LastHitTime = Time.time;
				StartCoroutine(LoseMoneyEffect());
			}
			else if (collision.collider.CompareTag("Enemy") && CanBeHit())
			{
				Health -= 50;
				StartCoroutine(TakeDamageEffect());
				LastHitTime = Time.time;
			}
        }

		private IEnumerator TakeDamageEffect()
		{
            SpriteRenderer.color = Color.red;
			yield return new WaitForSeconds(0.7f);
            SpriteRenderer.color = Color.white;
        }

        private IEnumerator LoseMoneyEffect()
        {
            GemText.color = Color.red;
            yield return new WaitForSeconds(0.7f);
            GemText.color = Color.white;
        }
    }
}
