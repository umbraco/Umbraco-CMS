import type { UmbPropertyValidationPathTranslator } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValidationPathTranslator<PropertyValueType>
	extends ManifestApi<UmbPropertyValidationPathTranslator<PropertyValueType>> {
	type: 'propertyValidationPathTranslator';
	forEditorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValidationPathTranslator: ManifestPropertyValidationPathTranslator<any>;
	}
}
