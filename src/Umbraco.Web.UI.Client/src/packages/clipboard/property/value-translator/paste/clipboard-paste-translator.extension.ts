import type { UmbClipboardPastePropertyValueTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardPastePropertyValueTranslator
	extends ManifestApi<UmbClipboardPastePropertyValueTranslator> {
	type: 'clipboardPastePropertyValueTranslator';
	fromClipboardEntryValueType: string;
	toPropertyEditorUi: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardPastePropertyValueTranslator: ManifestClipboardPastePropertyValueTranslator;
	}
}
