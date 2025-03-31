import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValidationPathTranslator',
		alias: 'Umb.PropertyValidationPathTranslator.BlockList',
		name: 'Block List Property Validation Path Translator',
		forEditorAlias: UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS,
		api: () => import('./block-list-validation-property-path-translator.api.js'),
	},
];
