public class Task : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public Guid? AssignedToId { get; set; }
    public User AssignedTo { get; set; }
}