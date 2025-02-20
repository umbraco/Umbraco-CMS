import { UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.Stylesheet.Tree.ReloadChildrenOf',
		name: 'Reload Stylesheet Tree Item Children Entity Action',
		forEntityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
	},
];
