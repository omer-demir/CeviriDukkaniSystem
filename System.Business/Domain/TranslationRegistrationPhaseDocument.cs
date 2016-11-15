using Tangent.CeviriDukkani.Domain.Dto.System;

namespace System.Business.Domain {
    public class TranslationRegistrationPhaseDocument {
        public Guid DocumentId { get; set; }
        public UserDto User { get; set; }
        public int Step { get; set; }
        public bool Finished { get; set; }
    }
}