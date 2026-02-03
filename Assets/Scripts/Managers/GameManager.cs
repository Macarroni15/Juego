using UnityEngine;
using System.Collections.Generic;

namespace CookingGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private List<RecipeSO> allRecipes;
        [SerializeField] private Transform customerSpawnPoint;
        [SerializeField] private GameObject customerPrefab;

        private int score;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SpawnCustomer();
        }

        public void SpawnCustomer()
        {
            Instantiate(customerPrefab, customerSpawnPoint.position, customerSpawnPoint.rotation);
        }

        public void AddScore(int amount)
        {
            score += amount;
            Debug.Log("Score: " + score);
        }
    }
}
