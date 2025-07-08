
using System.Collections.Generic;

/// <summary>
/// Tracks the history of expansion choices made by the player during a run.
/// </summary>
[System.Serializable]
public class ExpansionHistory
{
    public List<ExpansionOption> choices;

    public ExpansionHistory()
    {
        choices = new List<ExpansionOption>();
    }

    /// <summary>
    /// Adds a chosen expansion to the history.
    /// </summary>
    /// <param name="option">The ExpansionOption that was selected.</param>
    public void AddChoice(ExpansionOption option)
    {
        if (option != null)
        {
            choices.Add(option);
        }
    }
}