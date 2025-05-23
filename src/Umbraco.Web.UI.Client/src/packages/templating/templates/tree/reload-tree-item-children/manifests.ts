import { UMB_TEMPLATE_ROOT_ENTITY_TYPE, UMB_TEMPLATE_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.Template.Tree.ReloadChildrenOf',
		name: 'Reload Template Tree Item Children Entity Action',
		forEntityTypes: [UMB_TEMPLATE_ROOT_ENTITY_TYPE, UMB_TEMPLATE_ENTITY_TYPE],
	},
];
