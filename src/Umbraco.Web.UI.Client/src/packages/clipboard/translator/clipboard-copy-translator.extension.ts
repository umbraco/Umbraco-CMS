import type { UmbClipboardCopyTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestClipboardCopyTranslator extends ManifestApi<UmbClipboardCopyTranslator> {
	type: 'clipboardCopyTranslator';
	forPropertyEditorUiAliases: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbClipboardCopyTranslator: ManifestClipboardCopyTranslator;
	}
}
