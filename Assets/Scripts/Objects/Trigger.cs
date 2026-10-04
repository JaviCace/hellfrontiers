using System.Collections.Generic;
using UnityEngine;
using Enemies;
using Managers;

namespace Level
{

    /// <summary>
    /// Zona invisible que, al ser activada por el jugador, inicia un evento
    /// (típicamente activar puertas e iniciar una secuencia de jefe).
    /// </summary>
    public class InvisibleTrigger : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D zoneCollider;
        [SerializeField] private List<Door> doors = new List<Door>();
        [SerializeField] private BaseBoss boss;

        private void Awake()
        {
            if (zoneCollider != null) zoneCollider.isTrigger = true;
        }

        /// <summary>
        /// asigna el jefe que debe activarse
        /// </summary>
        public void SetBoss(BaseBoss b) => boss = b;

        /// <summary>
        /// asigna las puertas a activar
        /// </summary>
        public void SetDoors(List<Door> doorsList) => doors = doorsList;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) Llamar();
        }

        /// <summary>
        /// inicia la secuencia del jefe si este no ha sido derrotado
        /// </summary>
        public void Llamar()
        {
            if (boss == null || PlayerDataManager.IsBossDefeated(boss.bossName)) return;

            boss.PlayIntro();

            foreach (var door in doors)
            {
                if (door != null) door.gameObject.SetActive(true);
            }

            Destroy(gameObject); // destruir el trigger para que no se repita
        }
    }
}