using System.ComponentModel;

namespace Domain.Common;

public class BaseEntity
{
    public int Id { get; set; }

    public DateTime Created { get; set; }

    public string CreatedBy { get; set; } = "System";

    public DateTime? LastModified { get; set; }

    public string LastModifiedBy { get; set; } = "System";

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
}

