using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgendaTattoo.DTO.Public
{
    public class CreateAnamnesisFormRequest
    {
        public bool HasAllergies { get; set; }
        [StringLength(500)]
        public string? AllergiesDescription { get; set; }

        public bool HasSkinConditions { get; set; }
        [StringLength(500)]
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
        [StringLength(500)]
        public string? PreviousTattooReactionDescription { get; set; }

        [StringLength(500)]
        public string? CurrentMedications { get; set; }

        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }

        // Não uso [Required] aqui de propósito: em bool não-anulável o [Required] do DataAnnotations
        // não bloqueia "false" (que é um valor válido, não "ausente"). A checagem de
        // "precisa ser true" é feita explicitamente no AnamnesisService.
        public bool ConsentAccepted { get; set; }

        [Required, StringLength(150, MinimumLength = 2)]
        public string SignatureFullName { get; set; } = string.Empty;
    }
}
