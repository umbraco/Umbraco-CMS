import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';

export const manifests = [
	{
		type: 'propertyValueCloner',
		name: 'Block Grid Value Cloner',
		alias: 'Umb.PropertyValueCloner.BlockGrid',
		api: () => import('./property-value-cloner-block-grid.cloner.js'),
		forEditorAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
	},
];
