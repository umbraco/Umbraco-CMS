import type { UmbIntegerPropertyEditorUiValue } from './types.js';
import { umbResolveNumberPresetValue } from './resolve-number-preset-value.function.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbIntegerPropertyValuePreset implements UmbPropertyValuePreset<
	UmbIntegerPropertyEditorUiValue,
	UmbPropertyEditorConfig
> {
	async processValue(value: undefined | UmbIntegerPropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		if (value !== undefined && value !== null) {
			return value;
		}
		const fallbackValue = umbResolveNumberPresetValue(config);
		if (fallbackValue !== undefined) {
			return Math.round(fallbackValue);
		}
		return undefined;
	}

	destroy(): void {}
}

export { UmbIntegerPropertyValuePreset as api };
