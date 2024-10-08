﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace _2Sport_BE.Repository.Models;

[Table("CartItems")]
public class CartItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("CartId")]
    public int CartId { get; set; }

    public int? Quantity { get; set; }

    [Column("TotalPrice", TypeName = "decimal")]
    public decimal? TotalPrice { get; set; }

    [Column("WarehouseId")]
    public int? WarehouseId { get; set; }

    public bool? Status { get; set; }

    public virtual Cart Cart { get; set; }

    public virtual Warehouse Warehouse { get; set; }
}
