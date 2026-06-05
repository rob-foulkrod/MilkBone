using TodoApp.Models;
using TodoApp.Repositories;

namespace TodoApp.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<TodoItem> GetAllTodos() => _repository.GetAll();

    public IEnumerable<TodoItem> GetActiveTodos() => _repository.GetByCompleted(false);

    public IEnumerable<TodoItem> GetCompletedTodos() => _repository.GetByCompleted(true);

    public TodoItem? GetTodoById(int id) => _repository.GetById(id);

    public TodoItem CreateTodo(string title, string? description, TodoPriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));

        if (description?.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(description));

        var item = new TodoItem
        {
            Title = title.Trim(),
            Description = description?.Trim(),
            Priority = priority,
            DueDate = dueDate
        };

        return _repository.Add(item);
    }

    public TodoItem? UpdateTodo(int id, string title, string? description, TodoPriority priority, DateTime? dueDate, bool isCompleted)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));

        if (description?.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(description));

        var existing = _repository.GetById(id);
        if (existing is null) return null;

        existing.Title = title.Trim();
        existing.Description = description?.Trim();
        existing.Priority = priority;
        existing.DueDate = dueDate;
        existing.IsCompleted = isCompleted;

        return _repository.Update(existing);
    }

    public bool DeleteTodo(int id) => _repository.Delete(id);

    public TodoItem? ToggleComplete(int id)
    {
        var item = _repository.GetById(id);
        if (item is null) return null;

        item.IsCompleted = !item.IsCompleted;
        return _repository.Update(item);
    }

    public TodoStats GetStats()
    {
        var all = _repository.GetAll().ToList();
        var today = DateTime.UtcNow.Date;
        var active = all.Where(i => !i.IsCompleted).ToList();

        return new TodoStats(
            Total: all.Count,
            Active: active.Count,
            Completed: all.Count(i => i.IsCompleted),
            Overdue: active.Count(i => i.DueDate.HasValue && i.DueDate.Value.Date < today),
            DueToday: active.Count(i => i.DueDate.HasValue && i.DueDate.Value.Date == today)
        );
    }
}
