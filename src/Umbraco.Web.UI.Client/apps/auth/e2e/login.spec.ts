import { expect, test } from '@playwright/test';

test('login', async ({ page }) => {
	// Fill input[name="email"]
	await page.locator('input[name="email"]').fill('test@umbraco.com');

	// Fill input[name="password"]
	await page.locator('input[name="password"]').fill('test123456');

	// Listen for message on window containing type: 'login'
	const eventPromise = new Promise<void>((resolve) => {
		window.addEventListener(
			'message',
			(event) => {
				expect(event.data.type).toBe('login');
				resolve();
			},
			{ once: true }
		);
	});

	// Click [aria-label="Login"]
	await page.locator('[aria-label="Login"]').click();

	// Wait for message on window containing type: 'login'
	await eventPromise;
});
