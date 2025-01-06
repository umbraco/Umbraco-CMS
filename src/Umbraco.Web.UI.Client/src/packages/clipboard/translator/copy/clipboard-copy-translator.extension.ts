import type { UmbClipboardCopyTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardCopyTranslator extends ManifestApi<UmbClipboardCopyTranslator<any>> {
	type: 'clipboardCopyTranslator';
	fromPropertyEditorUi: string;
	toClipboardEntryValueType: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardCopyTranslator: ManifestClipboardCopyTranslator;
	}
}
