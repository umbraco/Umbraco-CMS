import { UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.Element.Tree.ReloadChildrenOf',
		name: 'Reload Element Tree Item Children Entity Action',
		forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	},
];
