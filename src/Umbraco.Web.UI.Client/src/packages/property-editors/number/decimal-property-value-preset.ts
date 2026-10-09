import type { UmbDecimalPropertyEditorUiValue } from './types.js';
import { umbResolveNumberPresetValue } from './resolve-number-preset-value.function.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbDecimalPropertyValuePreset implements UmbPropertyValuePreset<
	UmbDecimalPropertyEditorUiValue,
	UmbPropertyEditorConfig
> {
	async processValue(value: undefined | UmbDecimalPropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		if (value !== undefined && value !== null) {
			return value;
		}
		return umbResolveNumberPresetValue(config);
	}

	destroy(): void {}
}

export { UmbDecimalPropertyValuePreset as api };
