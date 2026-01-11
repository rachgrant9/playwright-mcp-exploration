import { test, expect } from '@playwright/test';

test.describe('Todo Application', () => {
  test('should add a new todo and mark it as complete', async ({ page }) => {
    // Navigate to the application
    await page.goto('http://localhost:5098');

    // Verify the page loaded correctly
    await expect(page).toHaveTitle('Todo List');
    await expect(page.getByRole('heading', { name: 'Todo List' })).toBeVisible();

    // Wait for initial load
    await expect(page.getByRole('heading', { name: 'Add New Todo' })).toBeVisible();

    // Add a new todo with unique title
    const todoTitle = `Buy groceries ${Date.now()}`;
    await page.getByRole('textbox', { name: 'Enter todo title' }).fill(todoTitle);
    await page.getByRole('button', { name: 'Add Todo' }).click();

    // Verify todo appears in the list with Pending status
    const todoItem = page.getByRole('listitem').filter({ hasText: todoTitle }).first();
    await expect(todoItem).toBeVisible();
    await expect(todoItem.getByText('Pending')).toBeVisible();

    // Mark todo as complete
    await todoItem.getByRole('checkbox').click();

    // Verify status changed to Completed (this ensures the update has completed)
    await expect(todoItem.getByText('Completed')).toBeVisible();
    
    // Verify checkbox is checked
    await expect(todoItem.getByRole('checkbox')).toBeChecked();
  });
});
