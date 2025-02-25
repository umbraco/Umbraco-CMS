import type { UmbTogglePropertyEditorUiValue } from './types.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbTrueFalsePropertyValuePreset
	implements UmbPropertyValuePreset<UmbTogglePropertyEditorUiValue, UmbPropertyEditorConfig>
{
	async processValue(value: undefined | UmbTogglePropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		const initialState = (config.find((x) => x.alias === 'default')?.value as boolean | undefined) ?? false;
		return value !== undefined ? value : initialState;
	}

	destroy(): void {}
}

export { UmbTrueFalsePropertyValuePreset as api };
