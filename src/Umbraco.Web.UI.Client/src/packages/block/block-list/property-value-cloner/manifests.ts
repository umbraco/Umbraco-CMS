import { UMB_BLOCK_LIST } from '../constants.js';

export const manifests = [
	{
		type: 'propertyValueCloner',
		name: 'test-cloner-1',
		alias: 'Umb.PropertyValueCloner.BlockList',
		api: () => import('./property-value-cloner-block-list.cloner.js'),
		forEditorAlias: UMB_BLOCK_LIST,
	},
];
