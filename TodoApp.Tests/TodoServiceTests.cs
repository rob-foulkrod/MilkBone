using TodoApp.Models;
using TodoApp.Repositories;
using TodoApp.Services;
using Xunit;

namespace TodoApp.Tests;

public class TodoServiceTests
{
    private static (TodoService service, ITodoRepository repo) Create()
    {
        var repo = new InMemoryTodoRepository();
        // Clear seed data for isolated tests
        foreach (var item in repo.GetAll().ToList())
            repo.Delete(item.Id);
        var svc = new TodoService(repo);
        return (svc, repo);
    }

    // ── CreateTodo ───────────────────────────────────────────────────────
    [Fact]
    public void CreateTodo_ReturnsNewItem()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("Buy milk", "2% please", TodoPriority.Medium, null);
        Assert.NotEqual(0, item.Id);
        Assert.Equal("Buy milk", item.Title);
        Assert.Equal("2% please", item.Description);
        Assert.Equal(TodoPriority.Medium, item.Priority);
        Assert.False(item.IsCompleted);
    }

    [Fact]
    public void CreateTodo_TrimsTitle()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("  Hello  ", null, TodoPriority.Low, null);
        Assert.Equal("Hello", item.Title);
    }

    [Fact]
    public void CreateTodo_Throws_WhenTitleEmpty()
    {
        var (svc, _) = Create();
        Assert.Throws<ArgumentException>(() => svc.CreateTodo("", null, TodoPriority.Low, null));
        Assert.Throws<ArgumentException>(() => svc.CreateTodo("   ", null, TodoPriority.Low, null));
    }

    [Fact]
    public void CreateTodo_Throws_WhenTitleTooLong()
    {
        var (svc, _) = Create();
        var longTitle = new string('x', 201);
        Assert.Throws<ArgumentException>(() => svc.CreateTodo(longTitle, null, TodoPriority.Low, null));
    }

    [Fact]
    public void CreateTodo_Throws_WhenDescriptionTooLong()
    {
        var (svc, _) = Create();
        var longDesc = new string('d', 1001);
        Assert.Throws<ArgumentException>(() => svc.CreateTodo("Title", longDesc, TodoPriority.Low, null));
    }

    [Fact]
    public void CreateTodo_AcceptsMaxLengthTitle()
    {
        var (svc, _) = Create();
        var maxTitle = new string('t', 200);
        var item = svc.CreateTodo(maxTitle, null, TodoPriority.Low, null);
        Assert.Equal(maxTitle, item.Title);
    }

    [Fact]
    public void CreateTodo_WithDueDate_PersistsDueDate()
    {
        var (svc, _) = Create();
        var due = DateTime.UtcNow.AddDays(3);
        var item = svc.CreateTodo("Task", null, TodoPriority.Low, due);
        Assert.NotNull(item.DueDate);
        Assert.Equal(due.Date, item.DueDate!.Value.Date);
    }

    // ── GetAllTodos / GetActiveTodos / GetCompletedTodos ─────────────────
    [Fact]
    public void GetAllTodos_ReturnsAllItems()
    {
        var (svc, _) = Create();
        svc.CreateTodo("A", null, TodoPriority.Low, null);
        svc.CreateTodo("B", null, TodoPriority.Low, null);
        Assert.Equal(2, svc.GetAllTodos().Count());
    }

    [Fact]
    public void GetActiveTodos_ExcludesCompleted()
    {
        var (svc, _) = Create();
        svc.CreateTodo("Active", null, TodoPriority.Low, null);
        var done = svc.CreateTodo("Done", null, TodoPriority.Low, null);
        svc.ToggleComplete(done.Id);   // mark complete

        var active = svc.GetActiveTodos().ToList();
        Assert.Single(active);
        Assert.Equal("Active", active[0].Title);
    }

    [Fact]
    public void GetCompletedTodos_IncludesOnlyCompleted()
    {
        var (svc, _) = Create();
        svc.CreateTodo("Active", null, TodoPriority.Low, null);
        var done = svc.CreateTodo("Done", null, TodoPriority.Low, null);
        svc.ToggleComplete(done.Id);

        var completed = svc.GetCompletedTodos().ToList();
        Assert.Single(completed);
        Assert.Equal("Done", completed[0].Title);
    }

    // ── GetTodoById ──────────────────────────────────────────────────────
    [Fact]
    public void GetTodoById_ReturnsItem()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("Find me", null, TodoPriority.Medium, null);
        var found = svc.GetTodoById(item.Id);
        Assert.NotNull(found);
        Assert.Equal(item.Id, found!.Id);
    }

    [Fact]
    public void GetTodoById_ReturnsNull_WhenMissing()
    {
        var (svc, _) = Create();
        Assert.Null(svc.GetTodoById(99999));
    }

    // ── UpdateTodo ───────────────────────────────────────────────────────
    [Fact]
    public void UpdateTodo_ChangesFields()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("Old Title", "Old desc", TodoPriority.Low, null);
        var updated = svc.UpdateTodo(item.Id, "New Title", "New desc", TodoPriority.High, null, false);
        Assert.NotNull(updated);
        Assert.Equal("New Title", updated!.Title);
        Assert.Equal("New desc", updated.Description);
        Assert.Equal(TodoPriority.High, updated.Priority);
    }

    [Fact]
    public void UpdateTodo_ReturnsNull_WhenNotExists()
    {
        var (svc, _) = Create();
        var result = svc.UpdateTodo(9999, "X", null, TodoPriority.Low, null, false);
        Assert.Null(result);
    }

    [Fact]
    public void UpdateTodo_Throws_WhenTitleEmpty()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("T", null, TodoPriority.Low, null);
        Assert.Throws<ArgumentException>(() => svc.UpdateTodo(item.Id, "", null, TodoPriority.Low, null, false));
    }

    [Fact]
    public void UpdateTodo_Throws_WhenTitleTooLong()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("T", null, TodoPriority.Low, null);
        var longTitle = new string('x', 201);
        Assert.Throws<ArgumentException>(() => svc.UpdateTodo(item.Id, longTitle, null, TodoPriority.Low, null, false));
    }

    // ── DeleteTodo ───────────────────────────────────────────────────────
    [Fact]
    public void DeleteTodo_RemovesItem_ReturnsTrue()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("Delete me", null, TodoPriority.Low, null);
        Assert.True(svc.DeleteTodo(item.Id));
        Assert.Null(svc.GetTodoById(item.Id));
    }

    [Fact]
    public void DeleteTodo_ReturnsFalse_WhenNotExists()
    {
        var (svc, _) = Create();
        Assert.False(svc.DeleteTodo(9999));
    }

    // ── ToggleComplete ───────────────────────────────────────────────────
    [Fact]
    public void ToggleComplete_MarksActiveAsComplete()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("Toggle me", null, TodoPriority.Low, null);
        var toggled = svc.ToggleComplete(item.Id);
        Assert.NotNull(toggled);
        Assert.True(toggled!.IsCompleted);
    }

    [Fact]
    public void ToggleComplete_MarksCompleteAsActive()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("Toggle me", null, TodoPriority.Low, null);
        svc.ToggleComplete(item.Id);   // -> completed
        var toggled = svc.ToggleComplete(item.Id);  // -> active again
        Assert.NotNull(toggled);
        Assert.False(toggled!.IsCompleted);
    }

    [Fact]
    public void ToggleComplete_ReturnsNull_WhenNotExists()
    {
        var (svc, _) = Create();
        Assert.Null(svc.ToggleComplete(9999));
    }

    // ── GetStats ─────────────────────────────────────────────────────────
    [Fact]
    public void GetStats_ReturnsCorrectCounts()
    {
        var (svc, _) = Create();
        svc.CreateTodo("Active1", null, TodoPriority.Low, null);
        svc.CreateTodo("Active2", null, TodoPriority.Low, null);
        var done = svc.CreateTodo("Done", null, TodoPriority.Low, null);
        svc.ToggleComplete(done.Id);

        var stats = svc.GetStats();
        Assert.Equal(3, stats.Total);
        Assert.Equal(2, stats.Active);
        Assert.Equal(1, stats.Completed);
    }

    [Fact]
    public void GetStats_CountsOverdue()
    {
        var (svc, _) = Create();
        svc.CreateTodo("Overdue", null, TodoPriority.Low, DateTime.UtcNow.AddDays(-2));
        svc.CreateTodo("Future", null, TodoPriority.Low, DateTime.UtcNow.AddDays(5));

        var stats = svc.GetStats();
        Assert.Equal(1, stats.Overdue);
    }

    [Fact]
    public void GetStats_CountsDueToday()
    {
        var (svc, _) = Create();
        svc.CreateTodo("Today", null, TodoPriority.Low, DateTime.UtcNow.Date);
        svc.CreateTodo("Tomorrow", null, TodoPriority.Low, DateTime.UtcNow.AddDays(1));

        var stats = svc.GetStats();
        Assert.Equal(1, stats.DueToday);
    }

    [Fact]
    public void GetStats_DoesNotCountCompletedAsOverdue()
    {
        var (svc, _) = Create();
        var item = svc.CreateTodo("Was overdue", null, TodoPriority.Low, DateTime.UtcNow.AddDays(-2));
        svc.ToggleComplete(item.Id);

        var stats = svc.GetStats();
        Assert.Equal(0, stats.Overdue);
    }
}
