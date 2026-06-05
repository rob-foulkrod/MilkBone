using TodoApp.Models;
using TodoApp.Repositories;
using Xunit;

namespace TodoApp.Tests;

public class InMemoryTodoRepositoryTests
{
    private static InMemoryTodoRepository CreateEmptyRepository()
    {
        // The default constructor seeds 2 items; we create and clear for isolation
        var repo = new InMemoryTodoRepository();
        foreach (var item in repo.GetAll().ToList())
            repo.Delete(item.Id);
        return repo;
    }

    // ── Add ──────────────────────────────────────────────────────────────
    [Fact]
    public void Add_AssignsIncrementingId()
    {
        var repo = CreateEmptyRepository();
        var a = repo.Add(new TodoItem { Title = "A" });
        var b = repo.Add(new TodoItem { Title = "B" });
        Assert.True(b.Id > a.Id);
    }

    [Fact]
    public void Add_SetsCreatedAt()
    {
        var repo = CreateEmptyRepository();
        var before = DateTime.UtcNow.AddSeconds(-1);
        var item = repo.Add(new TodoItem { Title = "X" });
        Assert.True(item.CreatedAt >= before);
    }

    [Fact]
    public void Add_ReturnsSavedItem()
    {
        var repo = CreateEmptyRepository();
        var item = repo.Add(new TodoItem { Title = "Hello", Priority = TodoPriority.High });
        Assert.Equal("Hello", item.Title);
        Assert.Equal(TodoPriority.High, item.Priority);
    }

    // ── GetById ──────────────────────────────────────────────────────────
    [Fact]
    public void GetById_ReturnsItem_WhenExists()
    {
        var repo = CreateEmptyRepository();
        var added = repo.Add(new TodoItem { Title = "Find me" });
        var found = repo.GetById(added.Id);
        Assert.NotNull(found);
        Assert.Equal(added.Id, found.Id);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenNotExists()
    {
        var repo = CreateEmptyRepository();
        Assert.Null(repo.GetById(9999));
    }

    // ── GetAll ───────────────────────────────────────────────────────────
    [Fact]
    public void GetAll_ReturnsAllItems()
    {
        var repo = CreateEmptyRepository();
        repo.Add(new TodoItem { Title = "T1" });
        repo.Add(new TodoItem { Title = "T2" });
        Assert.Equal(2, repo.GetAll().Count());
    }

    [Fact]
    public void GetAll_ActiveItemsBeforeCompleted()
    {
        var repo = CreateEmptyRepository();
        var a = repo.Add(new TodoItem { Title = "Active" });
        var b = repo.Add(new TodoItem { Title = "Done", IsCompleted = true });
        var items = repo.GetAll().ToList();
        Assert.Equal(a.Id, items.First().Id);
        Assert.Equal(b.Id, items.Last().Id);
    }

    // ── Update ───────────────────────────────────────────────────────────
    [Fact]
    public void Update_ModifiesTitle()
    {
        var repo = CreateEmptyRepository();
        var item = repo.Add(new TodoItem { Title = "Old" });
        item.Title = "New";
        var updated = repo.Update(item);
        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Title);
        Assert.Equal("New", repo.GetById(item.Id)!.Title);
    }

    [Fact]
    public void Update_SetsCompletedAt_WhenMarkedComplete()
    {
        var repo = CreateEmptyRepository();
        var item = repo.Add(new TodoItem { Title = "Task" });
        item.IsCompleted = true;
        var updated = repo.Update(item);
        Assert.True(updated!.IsCompleted);
        Assert.NotNull(updated.CompletedAt);
    }

    [Fact]
    public void Update_ClearsCompletedAt_WhenReactivated()
    {
        var repo = CreateEmptyRepository();
        var item = repo.Add(new TodoItem { Title = "Task", IsCompleted = true });
        item.IsCompleted = true;
        repo.Update(item);   // mark done
        item.IsCompleted = false;
        var updated = repo.Update(item);
        Assert.False(updated!.IsCompleted);
        Assert.Null(updated.CompletedAt);
    }

    [Fact]
    public void Update_ReturnsNull_WhenNotExists()
    {
        var repo = CreateEmptyRepository();
        var ghost = new TodoItem { Id = 9999, Title = "Ghost" };
        Assert.Null(repo.Update(ghost));
    }

    // ── Delete ───────────────────────────────────────────────────────────
    [Fact]
    public void Delete_RemovesItem_ReturnsTrue()
    {
        var repo = CreateEmptyRepository();
        var item = repo.Add(new TodoItem { Title = "Bye" });
        var result = repo.Delete(item.Id);
        Assert.True(result);
        Assert.Null(repo.GetById(item.Id));
    }

    [Fact]
    public void Delete_ReturnsFalse_WhenNotExists()
    {
        var repo = CreateEmptyRepository();
        Assert.False(repo.Delete(9999));
    }

    // ── GetByCompleted ───────────────────────────────────────────────────
    [Fact]
    public void GetByCompleted_FiltersCorrectly()
    {
        var repo = CreateEmptyRepository();
        repo.Add(new TodoItem { Title = "Active" });
        var done = repo.Add(new TodoItem { Title = "Done" });
        done.IsCompleted = true;
        repo.Update(done);

        var active = repo.GetByCompleted(false).ToList();
        var completed = repo.GetByCompleted(true).ToList();

        Assert.Single(active);
        Assert.Equal("Active", active[0].Title);
        Assert.Single(completed);
        Assert.Equal("Done", completed[0].Title);
    }

    // ── GetByPriority ────────────────────────────────────────────────────
    [Fact]
    public void GetByPriority_FiltersCorrectly()
    {
        var repo = CreateEmptyRepository();
        repo.Add(new TodoItem { Title = "High", Priority = TodoPriority.High });
        repo.Add(new TodoItem { Title = "Low", Priority = TodoPriority.Low });

        var high = repo.GetByPriority(TodoPriority.High).ToList();
        Assert.Single(high);
        Assert.Equal("High", high[0].Title);
    }
}
