import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';

export const manifests = [
	{
		type: 'propertyValueCloner',
		name: 'Block Single Value Cloner',
		alias: 'Umb.PropertyValueCloner.BlockSingle',
		api: () => import('./property-value-cloner-block-single.cloner.js'),
		forEditorAlias: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	},
];
