import type { UmbIntegerPropertyEditorUiValue } from './types.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbIntegerPropertyValuePreset
	implements UmbPropertyValuePreset<UmbIntegerPropertyEditorUiValue, UmbPropertyEditorConfig>
{
	async processValue(value: undefined | UmbIntegerPropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		const min = Number(config.find((x) => x.alias === 'minVal') ?? 0);
		const minVerified = isNaN(min) ? 0 : Math.round(min);

		return value !== undefined ? value : minVerified;
	}

	destroy(): void {}
}

export { UmbIntegerPropertyValuePreset as api };
