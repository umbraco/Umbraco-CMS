/* eslint-disable @typescript-eslint/no-explicit-any */
/**
 * Checks if the current environment is a test environment.
 * This function checks for various indicators that suggest the code is running in a test environment, such as Playwright or specific test flags.
 * @returns {boolean} True if the environment is a test environment, false otherwise.
 */
export function isTestEnvironment(): boolean {
	return (
		typeof window === 'undefined' ||
		// Check if running under Playwright
		typeof (window as any).__playwright__binding__ !== 'undefined'
	);
}
