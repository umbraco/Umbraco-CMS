import type { UmbClipboardCopyPropertyValueTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardCopyPropertyValueTranslator
	extends ManifestApi<UmbClipboardCopyPropertyValueTranslator<any>> {
	type: 'clipboardCopyPropertyValueTranslator';
	fromPropertyEditorUi: string;
	toClipboardEntryValueType: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardCopyPropertyValueTranslator: ManifestClipboardCopyPropertyValueTranslator;
	}
}
