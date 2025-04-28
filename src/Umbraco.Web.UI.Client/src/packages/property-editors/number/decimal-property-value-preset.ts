import type { UmbDecimalPropertyEditorUiValue } from './types.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbDecimalPropertyValuePreset
	implements UmbPropertyValuePreset<UmbDecimalPropertyEditorUiValue, UmbPropertyEditorConfig>
{
	async processValue(value: undefined | UmbDecimalPropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		const min = Number(config.find((x) => x.alias === 'minVal') ?? 0);
		const minVerified = isNaN(min) ? 0 : min;

		return value !== undefined ? value : minVerified;
	}

	destroy(): void {}
}

export { UmbDecimalPropertyValuePreset as api };
