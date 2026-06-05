using TodoApp.Models;

namespace TodoApp.Repositories;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly List<TodoItem> _items = [];
    private int _nextId = 1;

    public InMemoryTodoRepository()
    {
        // Seed with a couple of sample todos
        Add(new TodoItem
        {
            Title = "Welcome to MilkBone Todo!",
            Description = "This is your first todo item. Click edit to update it or check it off when done.",
            Priority = TodoPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1)
        });
        Add(new TodoItem
        {
            Title = "Explore the app",
            Description = "Create, edit, and complete your todos. Use priorities and due dates to stay organized.",
            Priority = TodoPriority.Medium
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
