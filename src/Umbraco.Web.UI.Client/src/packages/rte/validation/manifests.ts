import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValidationPathTranslator',
		alias: 'Umb.PropertyValidationPathTranslator.RTE',
		name: 'Rich Text Property Validation Path Translator',
		forEditorAlias: UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
		api: () => import('./rte-validation-property-path-translator.api.js'),
	},
];
