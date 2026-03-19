
using UnityEngine;

[CreateAssetMenu(fileName = "City Settings", menuName = "New City Settings")]
public class CitySettings : ScriptableObject
{
    public int money;
    public int day;
    public int curPopulation;
    public int curJobs;
    public int curFood;
    public int maxPopulation;
    public int maxJobs;
    public int incomePerJobs;
}
