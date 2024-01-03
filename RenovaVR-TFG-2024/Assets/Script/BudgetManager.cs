using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BudgetManager : MonoBehaviour
{
    public TMP_Text budgetText;
    public int initialBudget = 1000; // Initial budget
    private int currentBudget;

    private void Start()
    {
        currentBudget = initialBudget;
        UpdateBudgetText();
    }

    public bool CanAfford()
    {
        return currentBudget >= 100;
    }

    public void AddBudget()
    {
        currentBudget += 100;
        UpdateBudgetText();
    }

    public void RemoveBudget()
    {
        if (CanAfford())
        {
            currentBudget -= 100;
            UpdateBudgetText();
        }
        else
        {
            Debug.Log("Not enough budget.");
        }
    }

    private void UpdateBudgetText()
    {
        budgetText.text = "Budget: $" + currentBudget.ToString();
    }
}
