import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

/**
 * Parses a numeric range bound.
 * @param {unknown} value - The bound value to parse.
 * @returns {number | undefined} The parsed number, or `undefined` if not set or not a valid number.
 */
function parseBound(value: unknown): number | undefined {
	if (value === undefined || value === null || value === '') {
		return undefined;
	}
	const num = Number(value);
	return Number.isFinite(num) ? num : undefined;
}

/**
 * Resolves the value a numeric property should be preset with, based on its `validationRange` configuration.
 * @param {UmbPropertyEditorConfig} config - The property editor configuration.
 * @returns {number | undefined} The value to preset, or `undefined` to leave the field empty.
 */
export function umbResolveNumberPresetValue(config: UmbPropertyEditorConfig): number | undefined {
	const range = config.find((x) => x.alias === 'validationRange')?.value as
		| { min?: unknown; max?: unknown }
		| null
		| undefined;

	const min = parseBound(range?.min);
	if (min !== undefined && min > 0) {
		return min;
	}

	const max = parseBound(range?.max);
	if (max !== undefined && max < 0) {
		return max;
	}

	return undefined;
}
