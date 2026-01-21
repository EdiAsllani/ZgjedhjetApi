using System.ComponentModel.DataAnnotations;

namespace ZgjedhjetApi.Models.Entities
{
    public class Zgjedhjet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Kategoria { get; set; } = string.Empty;


        [Required]
        public string Komuna { get; set; } = string.Empty;


        [Required]
        public string Qendra_e_Votimit { get; set; } = string.Empty;

        
        [Required]
        public string VendVotimi { get; set; } = string.Empty;

        public int Partia111 { get; set; }
        public int Partia112 { get; set; }
        public int Partia113 { get; set; }
        public int Partia114 { get; set; }
        public int Partia115 { get; set; }
        public int Partia116 { get; set; }
        public int Partia117 { get; set; }
        public int Partia118 { get; set; }
        public int Partia119 { get; set; }
        public int Partia120 { get; set; }
        public int Partia121 { get; set; }
        public int Partia122 { get; set; }
        public int Partia123 { get; set; }
        public int Partia124 { get; set; }
        public int Partia125 { get; set; }
        public int Partia126 { get; set; }
        public int Partia127 { get; set; }
        public int Partia128 { get; set; }
        public int Partia129 { get; set; }
        public int Partia130 { get; set; }
        public int Partia131 { get; set; }
        public int Partia132 { get; set; }
        public int Partia133 { get; set; }
        public int Partia134 { get; set; }
        public int Partia135 { get; set; }
        public int Partia136 { get; set; }
        public int Partia137 { get; set; }
        public int Partia138 { get; set; }
    }
}
