using TodoApp.Models;

namespace TodoApp.Repositories;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly List<TodoItem> _items = [];
    private int _nextId = 1;

    public InMemoryTodoRepository()
    {
        // Seeded from GitHub Issues
        Add(new TodoItem
        {
            Title = "App doesn't work",
            Description = "Milkbone refuses to start, crashes immediately, or freezes. Expected: launch without drama, actually do things, not emotionally abandon the user. (Issue #3)",
            Priority = TodoPriority.Critical
        });
        Add(new TodoItem
        {
            Title = "Real time data Integration missing between EA Tools",
            Description = "Real time data integration is missing between EA Tools. (Issue #4)",
            Priority = TodoPriority.High
        });
        Add(new TodoItem
        {
            Title = "Text Notifications Not Working Correctly",
            Description = "Reminder notifications are not being sent via text. Example: reminder to buy kitty litter did not trigger a text message. (Issue #2)",
            Priority = TodoPriority.High
        });
        Add(new TodoItem
        {
            Title = "Bug: My code has an attitude",
            Description = "Processing pipeline logs escalating warnings and errors instead of handling requests gracefully. Investigate and resolve the attitude problem. (Issue #6)",
            Priority = TodoPriority.Medium
        });
        Add(new TodoItem
        {
            Title = "Add description to README",
            Description = "The README file requires a proper description outlining the application's functionality. (Issue #7)",
            Priority = TodoPriority.Low
        });
        Add(new TodoItem
        {
            Title = "Study Time",
            Description = "Schedule dedicated study time for the Microsoft certification exam. (Issue #5)",
            Priority = TodoPriority.Low
        });
    }

    public IEnumerable<TodoItem> GetAll() =>
        _items.OrderBy(i => i.IsCompleted)
              .ThenByDescending(i => i.Priority)
              .ThenBy(i => i.DueDate ?? DateTime.MaxValue)
              .Select(Clone)
              .ToList();

    public TodoItem? GetById(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        return item is null ? null : Clone(item);
    }

    public TodoItem Add(TodoItem item)
    {
        item.Id = _nextId++;
        item.CreatedAt = DateTime.UtcNow;
        _items.Add(Clone(item));
        return Clone(item);
    }

    public TodoItem? Update(TodoItem item)
    {
        var existing = _items.FirstOrDefault(i => i.Id == item.Id);
        if (existing is null) return null;

        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.Priority = item.Priority;
        existing.DueDate = item.DueDate;

        if (item.IsCompleted && !existing.IsCompleted)
        {
            existing.IsCompleted = true;
            existing.CompletedAt = DateTime.UtcNow;
        }
        else if (!item.IsCompleted && existing.IsCompleted)
        {
            existing.IsCompleted = false;
            existing.CompletedAt = null;
        }

        return Clone(existing);
    }

    public bool Delete(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null) return false;
        _items.Remove(item);
        return true;
    }

    public IEnumerable<TodoItem> GetByCompleted(bool isCompleted) =>
        _items.Where(i => i.IsCompleted == isCompleted)
              .OrderByDescending(i => i.Priority)
              .Select(Clone)
              .ToList();

    public IEnumerable<TodoItem> GetByPriority(TodoPriority priority) =>
        _items.Where(i => i.Priority == priority).Select(Clone).ToList();

    private static TodoItem Clone(TodoItem src) => new()
    {
        Id = src.Id,
        Title = src.Title,
        Description = src.Description,
        IsCompleted = src.IsCompleted,
        CreatedAt = src.CreatedAt,
        DueDate = src.DueDate,
        Priority = src.Priority,
        CompletedAt = src.CompletedAt
    };
}
