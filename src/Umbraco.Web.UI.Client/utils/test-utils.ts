export const defaultA11yConfig = {
	ignoredRules: [],
};

export type UmbTestRunnerWindow = Window &
	typeof globalThis & {
		__UMBRACO_TEST_RUN_A11Y_TEST: boolean;
	};

/**
 * Swallows "ResizeObserver loop completed with undelivered notifications" errors for the duration of a
 * test. UUI components that measure themselves — `uui-tab-group` observes itself and every tab — provoke
 * these, and the test runner turns any uncaught error into a failure for every test in the file.
 * @returns {() => void} Restores the previous handler.
 */
export function ignoreResizeObserverLoopErrors(): () => void {
	const previousHandler = window.onerror;

	window.onerror = (event, source, lineno, colno, error) => {
		if (typeof event === 'string' && event.includes('ResizeObserver loop')) return true;
		return previousHandler?.call(window, event, source, lineno, colno, error) ?? false;
	};

	return () => {
		window.onerror = previousHandler;
	};
}
