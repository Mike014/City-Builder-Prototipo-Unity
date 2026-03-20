using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    // Riferimento diretto al componente testuale dell'interfaccia.
    public TextMeshProUGUI statsText;

    // Riferimento allo ScriptableObject che funge da database dei dati correnti.
    [SerializeField] private CitySettings _citySettings;

    // Formatta e aggiorna la UI testuale con i dati aggiornati del turno.
    void UpdateStatText(ResourceAmount resource)
    {
        Debug.Log("UpdateStatText chiamato");
        // Utilizza string.Format per allocare la stringa con i dati attuali
        statsText.text = string.Format(
            "Day: {0}   Money: ${1}   Pop: {2} / {3}   Jobs : {4} / {5}   Food: {6}",
            new object[7] {
                _citySettings.day,
                _citySettings.money,
                _citySettings.curPopulation,
                _citySettings.maxPopulation,
                _citySettings.curJobs,
                _citySettings.maxJobs,
                _citySettings.curFood
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

// TODO: usare i dati di ResourceAmount invece di _citySettings
// per completare il disaccoppiamento dalla UI