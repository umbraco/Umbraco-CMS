import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.DocumentBlueprint.Tree.ReloadChildrenOf',
		name: 'Reload Document Blueprint Tree Item Children Entity Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {},
	},
];
