import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValidationPathTranslator',
		alias: 'Umb.PropertyValidationPathTranslator.BlockSingle',
		name: 'Block Single Property Validation Path Translator',
		forEditorAlias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
		api: () => import('./block-single-validation-property-path-translator.api.js'),
	},
];
