import type { UmbPropertyEditorUIToggleValue } from './types.js';
import type { UmbPropertyValuePresetApi } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbPropertyValuePresetTrueFalseApi
	implements UmbPropertyValuePresetApi<UmbPropertyEditorUIToggleValue, UmbPropertyEditorConfig>
{
	async processValue(value: undefined | UmbPropertyEditorUIToggleValue, config: UmbPropertyEditorConfig) {
		const initialState = (config.find((x) => x.alias === 'default')?.value as boolean | undefined) ?? false;
		return value !== undefined ? value : initialState;
	}

	destroy(): void {}
}

export { UmbPropertyValuePresetTrueFalseApi as api };
