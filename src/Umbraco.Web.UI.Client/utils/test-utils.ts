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

/**
 * Loads a UUI theme stylesheet on top of the light theme the test runner starts with.
 * @param {'dark' | 'high-contrast'} theme - The UUI theme to load.
 * @returns {Promise<() => void>} Removes the theme again.
 */
export async function useUuiTheme(theme: 'dark' | 'high-contrast'): Promise<() => void> {
	const link = document.createElement('link');
	link.rel = 'stylesheet';
	link.href = `/node_modules/@umbraco-ui/uui/dist/themes/${theme}.css`;
	const loaded = new Promise((resolve, reject) => {
		link.addEventListener('load', resolve, { once: true });
		link.addEventListener('error', reject, { once: true });
	});
	document.head.append(link);
	await loaded;
	return () => link.remove();
}

/**
 * Resolves any CSS colour, including `color-mix()` and `color(srgb …)`, to 8-bit sRGB channels.
 * @param {string} color - A computed CSS colour.
 * @returns {[number, number, number]} The red, green and blue channels.
 */
export function toSrgb(color: string): [number, number, number] {
	const context = document.createElement('canvas').getContext('2d', { willReadFrequently: true })!;
	context.fillStyle = color;
	context.fillRect(0, 0, 1, 1);
	const [red, green, blue] = context.getImageData(0, 0, 1, 1).data;
	return [red, green, blue];
}

/**
 * The WCAG 2 contrast ratio between two opaque CSS colours.
 * @param {string} first - A computed CSS colour.
 * @param {string} second - Another computed CSS colour.
 * @returns {number} The contrast ratio, from 1 to 21.
 */
export function contrastRatio(first: string, second: string): number {
	const luminance = (color: string) => {
		const [red, green, blue] = toSrgb(color).map((channel) => {
			const value = channel / 255;
			return value <= 0.03928 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4;
		});
		return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
	};
	const [lighter, darker] = [luminance(first), luminance(second)].sort((a, b) => b - a);
	return (lighter + 0.05) / (darker + 0.05);
}
