import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.MemberType.Tree.ReloadChildrenOf',
		name: 'Reload Member Type Tree Item Children Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
	},
];
