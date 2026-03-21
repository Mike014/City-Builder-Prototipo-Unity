using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI statsText;

    void UpdateStatText(ResourceAmount resource)
    {
        statsText.text = string.Format(
            "Day: {0}   Money: ${1}   Pop: {2} / {3}   Jobs : {4} / {5}   Food: {6}",
            new object[7] {
                resource.day,
                resource.money,
                resource.population,
                resource.maxPopulation,
                resource.jobs,
                resource.maxJobs,
                resource.food
            }
        );
    }

    public void OnEnable()
    {
        EventBus.OnResourceUpdated += UpdateStatText;
    }

    public void OnDisable()
    {
        EventBus.OnResourceUpdated -= UpdateStatText;
    }
}
