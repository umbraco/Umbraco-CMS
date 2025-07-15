import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValidationPathTranslator',
		alias: 'Umb.PropertyValidationPathTranslator.BlockGrid',
		name: 'Block Grid Property Validation Path Translator',
		forEditorAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
		api: () => import('./block-grid-validation-property-path-translator.api.js'),
	},
];
