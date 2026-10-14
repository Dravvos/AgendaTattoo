using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public class AnamnesisForm
    {
        public Guid Id { get; set; }

        public Guid StudioId { get; set; }
        public Studio Studio { get; set; } = null!;

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Guid AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

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

        /// <summary>Cópia do texto de consentimento exibido no momento do envio.</summary>
        public string ConsentText { get; set; } = string.Empty;
        public bool ConsentAccepted { get; set; }

        /// <summary>Nome completo digitado pelo cliente como assinatura eletrônica simples (não é uma assinatura digital certificada).</summary>
        public string SignatureFullName { get; set; } = string.Empty;

        public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? SubmittedFromIp { get; set; }
    }
}
