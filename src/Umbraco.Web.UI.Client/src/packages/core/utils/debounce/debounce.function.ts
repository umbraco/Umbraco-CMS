export interface UmbDebouncedFunction<T extends (...args: Array<any>) => any> {
	(...args: Parameters<T>): void;
	cancel(): void;
}

/**
 *
 * @param fn
 * @param ms
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
