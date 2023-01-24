import { expect, test } from '@playwright/test';

test('login', async ({ page }) => {
	await page.goto('/');

	// Fill input[name="email"]
	await page.locator('input[name="email"]').fill('test@umbraco.com');

	// Fill input[name="password"]
	await page.locator('input[name="password"]').fill('test123456');

	// Wait for console.log to be called
	page.on('console', (message) => {
		expect(message.text()).toBe('login');
	});

	// Click [aria-label="Login"]
	await page.locator('[aria-label="Login"]').click();
});
