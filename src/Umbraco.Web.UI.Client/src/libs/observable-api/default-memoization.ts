import { naiveObjectComparison } from './naive-object-comparison.js';

export function defaultMemoization(previousValue: any, currentValue: any): boolean {
	if (typeof previousValue === 'object' && typeof currentValue === 'object') {
		return naiveObjectComparison(previousValue, currentValue);
	}
	return previousValue === currentValue;
}
