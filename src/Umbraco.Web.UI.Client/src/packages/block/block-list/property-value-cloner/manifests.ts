import { UMB_BLOCK_LIST } from '../constants.js';

export const manifests = [
	{
		type: 'propertyValueCloner',
		name: 'Block List Value Cloner',
		alias: 'Umb.PropertyValueCloner.BlockList',
		api: () => import('./property-value-cloner-block-list.cloner.js'),
		forEditorAlias: UMB_BLOCK_LIST,
	},
];
