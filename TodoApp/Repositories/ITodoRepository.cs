using TodoApp.Models;

namespace TodoApp.Repositories;

public interface ITodoRepository
{
    IEnumerable<TodoItem> GetAll();
    TodoItem? GetById(int id);
    TodoItem Add(TodoItem item);
    TodoItem? Update(TodoItem item);
    bool Delete(int id);
    IEnumerable<TodoItem> GetByCompleted(bool isCompleted);
    IEnumerable<TodoItem> GetByPriority(TodoPriority priority);
}
