import type { UmbPropertyValidationPathTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValidationPathTranslator extends ManifestApi<UmbPropertyValidationPathTranslator> {
	type: 'propertyValidationPathTranslator';
	forEditorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValidationPathTranslator: ManifestPropertyValidationPathTranslator;
	}
}
