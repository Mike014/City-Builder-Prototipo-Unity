
using UnityEngine;

[CreateAssetMenu(fileName = "CityConfig", menuName = "New CityConfig")]
public class CityConfig : ScriptableObject
{
    public int startingMoney;
    public int startingDay;
    public int startingPopulation;
    public int incomePerJobs;
}


