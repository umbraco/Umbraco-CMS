import { UMB_DICTIONARY_ROOT_ENTITY_TYPE, UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.Dictionary.Tree.ReloadChildrenOf',
		name: 'Reload Dictionary Tree Item Children Entity Action',
		forEntityTypes: [UMB_DICTIONARY_ROOT_ENTITY_TYPE, UMB_DICTIONARY_ENTITY_TYPE],
	},
];
