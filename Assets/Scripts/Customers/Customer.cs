using UnityEngine;

namespace CookingGame
{
    public class Customer : MonoBehaviour
    {
        [SerializeField] private HealthProblem healthProblem;
        [SerializeField] private float patienceTime = 30f;
        private float currentPatience;

        private void Start()
        {
            currentPatience = patienceTime;
            // Randomly assign a health problem if not set
            if (healthProblem == 0) healthProblem = (HealthProblem)Random.Range(0, 3);
            Debug.Log("Customer arrived with: " + healthProblem);
        }

        private void Update()
        {
            currentPatience -= Time.deltaTime;
            if (currentPatience <= 0)
            {
                Debug.Log("Customer left unhappy.");
                Destroy(gameObject);
            }
        }

        public HealthProblem GetProblem() => healthProblem;

        public void DeliverFood(RecipeSO recipe)
        {
            if (recipe.recommendedFor == healthProblem)
            {
                Debug.Log("Correct food delivered! Customer is happy.");
                // Add score logic here
            }
            else
            {
                Debug.Log("Wrong food delivered! Customer is upset.");
            }
            Destroy(gameObject);
        }
    }
}
