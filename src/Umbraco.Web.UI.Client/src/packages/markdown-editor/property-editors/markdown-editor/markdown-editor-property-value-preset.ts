import type { UmbMarkdownPropertyEditorUiValue } from './types.js';
import type { UmbPropertyValuePreset } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbMarkdownPropertyValuePreset implements UmbPropertyValuePreset<UmbMarkdownPropertyEditorUiValue> {
	async processValue(value: undefined | UmbMarkdownPropertyEditorUiValue, config: UmbPropertyEditorConfig) {
		const defaultValue = config.find((x) => x.alias === 'defaultValue')?.value as string | undefined;
		return value !== undefined ? value : defaultValue;
	}

	destroy(): void {}
}

export { UmbMarkdownPropertyValuePreset as api };
