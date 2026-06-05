using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers;

public class TodoController : Controller
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    // GET /Todo
    public IActionResult Index(string? filter = null)
    {
        var todos = filter switch
        {
            "active" => _todoService.GetActiveTodos(),
            "completed" => _todoService.GetCompletedTodos(),
            _ => _todoService.GetAllTodos()
        };

        ViewBag.Filter = filter ?? "all";
        ViewBag.Stats = _todoService.GetStats();
        return View(todos);
    }

    // GET /Todo/Details/5
    public IActionResult Details(int id)
    {
        var todo = _todoService.GetTodoById(id);
        if (todo is null) return NotFound();
        return View(todo);
    }

    // GET /Todo/Create
    public IActionResult Create()
    {
        return View(new TodoItem());
    }

    // POST /Todo/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TodoItem model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            _todoService.CreateTodo(model.Title, model.Description, model.Priority, model.DueDate);
            TempData["Success"] = $"'{model.Title}' has been added to your list!";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET /Todo/Edit/5
    public IActionResult Edit(int id)
    {
        var todo = _todoService.GetTodoById(id);
        if (todo is null) return NotFound();
        return View(todo);
    }

    // POST /Todo/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TodoItem model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        try
        {
            var updated = _todoService.UpdateTodo(id, model.Title, model.Description, model.Priority, model.DueDate, model.IsCompleted);
            if (updated is null) return NotFound();
            TempData["Success"] = $"'{updated.Title}' has been updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET /Todo/Delete/5
    public IActionResult Delete(int id)
    {
        var todo = _todoService.GetTodoById(id);
        if (todo is null) return NotFound();
        return View(todo);
    }

    // POST /Todo/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var todo = _todoService.GetTodoById(id);
        var title = todo?.Title ?? "Item";
        var deleted = _todoService.DeleteTodo(id);
        if (!deleted) return NotFound();
        TempData["Success"] = $"'{title}' has been deleted.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Todo/Toggle/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int id, string? returnFilter)
    {
        var item = _todoService.ToggleComplete(id);
        if (item is null) return NotFound();

        TempData["Success"] = item.IsCompleted
            ? $"'{item.Title}' marked as complete! 🎉"
            : $"'{item.Title}' marked as active.";

        return RedirectToAction(nameof(Index), new { filter = returnFilter });
    }
}
