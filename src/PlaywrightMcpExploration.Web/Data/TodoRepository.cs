using Microsoft.EntityFrameworkCore;
using PlaywrightMcpExploration.Web.Models;

namespace PlaywrightMcpExploration.Web.Data;

public class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public TodoRepository(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Todo>> GetAllAsync()
    {
        return await _context.Todos.ToListAsync();
    }

    public async Task<Todo?> GetByIdAsync(int id)
    {
        return await _context.Todos.FindAsync(id);
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> UpdateAsync(int id, Todo todo)
    {
        var existingTodo = await _context.Todos.FindAsync(id);
        
        if (existingTodo == null)
        {
            return null;
        }

        existingTodo.Title = todo.Title;
        existingTodo.IsCompleted = todo.IsCompleted;
        
        await _context.SaveChangesAsync();
        return existingTodo;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        
        if (todo == null)
        {
            return false;
        }

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return true;
    }
}
