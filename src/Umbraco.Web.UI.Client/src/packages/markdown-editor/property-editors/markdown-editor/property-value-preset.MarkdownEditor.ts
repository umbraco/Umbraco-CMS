import type { UmbPropertyValuePresetApi } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUIMarkdownValue } from './types.js';

export class UmbPropertyValuePresetMarkdownApi implements UmbPropertyValuePresetApi<UmbPropertyEditorUIMarkdownValue> {
	async processValue(value: undefined | UmbPropertyEditorUIMarkdownValue, config: UmbPropertyEditorConfig) {
		const defaultValue = config.find((x) => x.alias === 'defaultvalue')?.value as string | undefined;
		return value !== undefined ? value : defaultValue;
	}

	destroy(): void {}
}

export { UmbPropertyValuePresetMarkdownApi as api };
