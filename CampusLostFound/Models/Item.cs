
using System;
using System.ComponentModel.DataAnnotations;

namespace CampusLostFound.Models
{
    public class Item
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Please enter the item name.")]
        [StringLength(100)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string ItemType { get; set; }

        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Lost / Found Date")]
        public DateTime ItemDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(150)]
        public string Location { get; set; }

        [Required(ErrorMessage = "Please enter the contact name.")]
        [StringLength(100)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Please enter contact information.")]
        [StringLength(100)]
        [Display(Name = "Contact Information")]
        public string ContactInfo { get; set; }

        public string ImagePath { get; set; }

        public string Status { get; set; }
    }
}
