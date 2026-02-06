/**
 * Compares two values for deep equality using JSON serialization with sorted
 * object keys. This makes the comparison independent of property insertion
 * order, which can differ between API endpoints returning the same data.
 * @param a - First value to compare.
 * @param b - Second value to compare.
 * @returns `true` if the values are deeply equal, `false` otherwise.
 */
export function umbDeepEqual(a: unknown, b: unknown): boolean {
	return sortedStringify(a) === sortedStringify(b);
}

function sortedStringify(value: unknown): string {
	return JSON.stringify(value, (_key, val) => {
		if (val && typeof val === 'object' && !Array.isArray(val)) {
			const sorted: Record<string, unknown> = {};
			for (const k of Object.keys(val).sort()) {
				sorted[k] = (val as Record<string, unknown>)[k];
			}
			return sorted;
		}
		return val;
	});
}
