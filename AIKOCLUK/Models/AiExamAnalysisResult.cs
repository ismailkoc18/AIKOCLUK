using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIKOCLUK.Models
{
    public class AiExamAnalysisResult
    {
        [JsonPropertyName("genelDegerlendirme")]
        public string GenelDegerlendirme { get; set; } = string.Empty;

        [JsonPropertyName("kirmiziAlarm")]
        public List<string> KirmiziAlarmDersleri { get; set; } = new();

        // Projenin başka bir yerinde KirmiziAlarm ismiyle çağrılıyorsa kırılmaması için alias:
        [JsonIgnore]
        public List<string> KirmiziAlarm => KirmiziAlarmDersleri;

        [JsonPropertyName("hedefKonular")]
        public List<TargetSubject> HedefKonular { get; set; } = new();

        [JsonPropertyName("haftalikOdakTavsiyesi")]
        public string HaftalikOdakTavsiyesi { get; set; } = string.Empty;
    }

    public class TargetSubject
    {
        [JsonPropertyName("ders")]
        public string Ders { get; set; } = string.Empty;

        [JsonPropertyName("konu")]
        public string Konu { get; set; } = string.Empty;

        [JsonPropertyName("neden")]
        public string Neden { get; set; } = string.Empty;

        [JsonPropertyName("taktik")]
        public string Taktik { get; set; } = string.Empty;
    }
}