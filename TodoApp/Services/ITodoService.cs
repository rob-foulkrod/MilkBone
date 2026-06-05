using TodoApp.Models;

namespace TodoApp.Services;

public interface ITodoService
{
    IEnumerable<TodoItem> GetAllTodos();
    IEnumerable<TodoItem> GetActiveTodos();
    IEnumerable<TodoItem> GetCompletedTodos();
    TodoItem? GetTodoById(int id);
    TodoItem CreateTodo(string title, string? description, TodoPriority priority, DateTime? dueDate);
    TodoItem? UpdateTodo(int id, string title, string? description, TodoPriority priority, DateTime? dueDate, bool isCompleted);
    bool DeleteTodo(int id);
    TodoItem? ToggleComplete(int id);
    TodoStats GetStats();
}
