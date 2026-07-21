import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

function parseNumberConfig(config: UmbPropertyEditorConfig, alias: string): number | undefined {
	const value = config.find((x) => x.alias === alias)?.value;
	if (value === undefined || value === null || value === '') {
		return undefined;
	}
	const num = Number(value);
	return Number.isFinite(num) ? num : undefined;
}

/**
 * Resolves the value a numeric property should be preset with, based on its `min`/`max` configuration.
 * @param {UmbPropertyEditorConfig} config - The property editor configuration.
 * @returns {number | undefined} The value to preset, or `undefined` to leave the field empty.
 */
export function umbResolveNumberPresetValue(config: UmbPropertyEditorConfig): number | undefined {
	const min = parseNumberConfig(config, 'min');
	if (min !== undefined && min > 0) {
		return min;
	}

	const max = parseNumberConfig(config, 'max');
	if (max !== undefined && max < 0) {
		return max;
	}

	return undefined;
}
