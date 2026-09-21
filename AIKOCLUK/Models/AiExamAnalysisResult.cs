using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIKOCLUK.Models
{
    public class AiExamAnalysisResult
    {
        [JsonPropertyName("genelDegerlendirme")]
        public string GenelDegerlendirme { get; set; }

        [JsonPropertyName("kirmiziAlarm")]
        public List<string> KirmiziAlarm { get; set; }

        [JsonPropertyName("hedefKonular")]
        public List<TargetSubject> HedefKonular { get; set; }

        [JsonPropertyName("haftalikOdakTavsiyesi")]
        public string HaftalikOdakTavsiyesi { get; set; }
    }

    public class TargetSubject
    {
        [JsonPropertyName("ders")]
        public string Ders { get; set; }

        [JsonPropertyName("konu")]
        public string Konu { get; set; }

        [JsonPropertyName("neden")]
        public string Neden { get; set; }

        [JsonPropertyName("taktik")]
        public string Taktik { get; set; }
    }
}