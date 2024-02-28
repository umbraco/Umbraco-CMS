import { jsonStringComparison } from './naive-object-comparison.function.js';

export function defaultMemoization(previousValue: any, currentValue: any): boolean {
	if (typeof previousValue === 'object' && typeof currentValue === 'object') {
		return jsonStringComparison(previousValue, currentValue);
	}
	return previousValue === currentValue;
}
