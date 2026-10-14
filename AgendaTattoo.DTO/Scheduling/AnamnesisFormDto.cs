using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.DTO.Scheduling
{
    /// <summary>Visão interna (autenticada) para a equipe do estúdio — dono ou o próprio artista do agendamento.</summary>
    public class AnamnesisFormDto
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        public bool HasAllergies { get; set; }
        public string? AllergiesDescription { get; set; }
        public bool HasSkinConditions { get; set; }
        public string? SkinConditionsDescription { get; set; }
        public bool HasDiabetes { get; set; }
        public bool HasBloodClottingDisorder { get; set; }
        public bool HasHeartCondition { get; set; }
        public bool HasEpilepsy { get; set; }
        public bool IsPregnantOrBreastfeeding { get; set; }
        public bool UsesBloodThinners { get; set; }
        public bool UsesIsotretinoin { get; set; }
        public bool HasKeloidHistory { get; set; }
        public bool ConsumedAlcoholOrDrugsLast24h { get; set; }
        public bool HadPreviousTattooReaction { get; set; }
        public string? PreviousTattooReactionDescription { get; set; }
        public string? CurrentMedications { get; set; }
        public string? AdditionalNotes { get; set; }

        public bool ConsentAccepted { get; set; }
        public string SignatureFullName { get; set; } = string.Empty;
        public DateTimeOffset SubmittedAt { get; set; }
    }
}
