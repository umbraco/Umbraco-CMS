export interface UmbDebouncedFunction<T extends (...args: Array<any>) => any> {
	(...args: Parameters<T>): void;
	cancel(): void;
}

/**
 * Creates a debounced version of the provided function that delays execution
 * until after the specified wait time has elapsed since the last invocation.
 * @param {T} fn - The function to debounce
 * @param {number} ms - Delay in milliseconds (default: 0)
 * @returns {UmbDebouncedFunction<T>} A debounced function with a cancel method
 */
export function debounce<T extends (...args: Array<any>) => any>(fn: T, ms = 0): UmbDebouncedFunction<T> {
	let timeoutId: ReturnType<typeof setTimeout> | undefined;

	const debounced = function (this: any, ...args: Parameters<T>) {
		clearTimeout(timeoutId);
		timeoutId = setTimeout(() => fn.apply(this, args), ms);
	} as UmbDebouncedFunction<T>;

	debounced.cancel = () => {
		clearTimeout(timeoutId);
		timeoutId = undefined;
	};

	return debounced;
}
