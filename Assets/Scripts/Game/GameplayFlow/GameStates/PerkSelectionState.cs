using System.Collections.Generic;
using UnityEngine;

public class PerkSelectionState : GameState
{
    public PerkSelectionState(GameplayManager context) : base(context) {}

    private List<Perk> offeredPerks;

    private Perk pendingPerk;

    private bool isPanelVisible;

    public override void EnterState()
    {
        offeredPerks = context.Perks.GetRandomPerks(context.Perks.PerkChoiceCount);

        if (offeredPerks.Count == 0)
        {
            Debug.LogWarning("No available perks to select from. Skipping perk selection.");

            context.MoveToNextQuestion();

            return;
        }

        context.UI.PerkSelected += OnPerkSelected;

        context.UI.PerkPanelToggled += OnPerkPanelToggle;

        context.UI.ShowPerkSelectionScreen(offeredPerks, context.PerkConfig);
        isPanelVisible = true;
        context.UI.SetPerkPanelToggleButtonsActive(isPanelVisible, true);
    }

    public override void ExitState()
    {
        context.UI.PerkSelected -= OnPerkSelected;

        context.UI.PerkPanelToggled -= OnPerkPanelToggle;

        context.UI.PerkUsed -= OnPerkDiscarded;

        context.UI.HidePerkSelectionScreen();
        isPanelVisible = true;
        context.UI.SetPerkPanelToggleButtonsActive(isPanelVisible, false);
    }

    private void OnPerkSelected(Perk selectedPerk)
    {
        if (selectedPerk == null)
        {
            Debug.LogError("Selected perk is null. Cannot add perk.");
            return;
        }

        if (selectedPerk.Type == Perk.PerkType.Active && context.Perks.IsInventoryFull)
        {
            pendingPerk = selectedPerk;

            context.UI.HidePerkSelectionScreen();
            isPanelVisible = false;
            context.UI.SetPerkPanelToggleButtonsActive(isPanelVisible, true);

            context.UI.ShowActiveCardInventory(context.Perks.ActivePerks, context.PerkConfig);

            context.UI.PerkUsed += OnPerkDiscarded;
        }
        else
        {
            context.Perks.AddPerk(selectedPerk);

            context.MoveToNextQuestion();
        }
    }

    private void OnPerkDiscarded(Perk oldPerk)
    {
        if (oldPerk == null || pendingPerk == null)
        {
            return;
        }

        context.Perks.RemovePerk(oldPerk);

        context.Perks.AddPerk(pendingPerk);

        pendingPerk = null;

        context.MoveToNextQuestion();
    }

    private void OnPerkPanelToggle()
    {
        pendingPerk = null;

        context.UI.PerkUsed -= OnPerkDiscarded;

        if (isPanelVisible)
        {            
            context.UI.ShowActiveCardInventory(context.Perks.ActivePerks, context.PerkConfig);

            context.UI.HidePerkSelectionScreen();
            isPanelVisible = false;
            context.UI.SetPerkPanelToggleButtonsActive(isPanelVisible, true);
        }
        else
        {
            context.UI.HideActiveCardInventory();

            context.UI.ShowPerkSelectionScreen(offeredPerks, context.PerkConfig);
            isPanelVisible = true;
            context.UI.SetPerkPanelToggleButtonsActive(isPanelVisible, true);
        }
    }
}
