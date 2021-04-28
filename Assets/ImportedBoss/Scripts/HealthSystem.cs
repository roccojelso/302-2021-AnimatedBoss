using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Jelsomeno
{

    public class HealthSystem : MonoBehaviour
    {

        public float health { get; private set; }
        public float healthMax = 100;

        public GameObject impactEffect;
        public Slider bossSlider;

        private void Start()
        {
            health = healthMax;
            HealthBarSetup();
        }

        void HealthBarSetup()
        {
            bossSlider.maxValue = health; // health bar is full
            bossSlider.value = health; // health bar can change
        }

        void CurrentHealth()
        {
            bossSlider.value = health; // gets the objects life throughout the game
        }

        public void TakeDamage(float amt)
        {
            if (amt <= 0) return;

            health -= amt;
            CurrentHealth(); // constantly setting health on the health bars


            if (health <= 0) Die();
        }

        public void Die()
        {
            GameObject effectIns = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(effectIns, 2f);

            if (health <= 0)
            {
                Destroy(gameObject);
            }


        }
    }
}

