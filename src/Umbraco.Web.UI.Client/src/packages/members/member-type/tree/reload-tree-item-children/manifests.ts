import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MemberType.Tree.ReloadChildrenOf',
		name: 'Reload Member Type Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
	},
];
