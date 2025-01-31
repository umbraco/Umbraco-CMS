import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';

export const manifests = [
	{
		type: 'propertyValueCloner',
		name: 'RTE Block Value Cloner',
		alias: 'Umb.PropertyValueCloner.BlockRte',
		api: () => import('./property-value-cloner-block-rte.cloner.js'),
		forEditorAlias: UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	},
];
