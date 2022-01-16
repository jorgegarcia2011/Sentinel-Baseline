﻿using Microsoft.Azure.Security.Detection.AlertContracts.V3;
using Microsoft.Azure.Sentinel.Analytics.Management.AnalyticsTemplatesService.Interface.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Sentinel.Analytics.Management.AnalyticsManagement.Contracts.Model.ARM.ModelValidation
{
    public class NoTechniquesWithoutMatchingTactics : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var ruleProperties = value as ScheduledTemplateInternalModel;
            if (ruleProperties?.RelevantTechniques != null)
            {
                foreach (AttackTechniques technique in ruleProperties?.RelevantTechniques)
                {
                    var asString = technique.ToString();
                    var correspondingTactics = KillChainTechniquesHelpers.GetCorrespondingKillChainIntent(asString).AsAttackTactics();
                    if (correspondingTactics.Count >= 0)
                    {
                        bool isTacticExists = correspondingTactics.Any((AttackTactic tactic) => ruleProperties?.Tactics?.Contains(tactic) ?? false);

                        if (!isTacticExists || correspondingTactics.Count == 0)
                        {
                            return new ValidationResult($"No valid tactic corresponding to the technique {technique} was provided in the tactics field.");
                        }
                    }
                }
            }

            return ValidationResult.Success;
        }
    }

    public static class AttackTacticExtensions
    {
        public static List<AttackTactic> AsAttackTactics(this KillChainIntent intent)
        {
            List<AttackTactic> tactics = new List<AttackTactic>();
            intent = intent.ClearObsoleteValues(); // Maps obsolete KillChainIntent values to newer ones in AlertContracts >= 2.5.0.0

            foreach (KillChainIntent value in Enum.GetValues(intent.GetType()))
            {
                if (intent.HasFlag(value))
                {
                    if (Enum.TryParse(value.ToString(), out AttackTactic correspondingTactic))
                    {
                        tactics.Add(correspondingTactic);
                    }
                }
            }

            return tactics;
        }
    }
}
