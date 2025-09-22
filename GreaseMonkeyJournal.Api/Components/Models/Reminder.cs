using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreaseMonkeyJournal.Api.Components.Models
{
    public class Reminder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // e.g., Repair, Maintenance
        
        public decimal? DueSpeedometerReading { get; set; } // Due reading in KM or Hours based on vehicle's speedometer type
    }
}
