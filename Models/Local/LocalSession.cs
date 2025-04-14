using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models.Local;

[Table("Sessions")]
public class LocalSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Email { get; set; }
    public DateTime LoginTimestamp { get; set; }
    public bool IsActive { get; set; }
}
