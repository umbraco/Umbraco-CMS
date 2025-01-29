import { UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.PartialView.Tree.ReloadChildrenOf',
		name: 'Reload Partial View Tree Item Children Entity Action',
		forEntityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
	},
];
