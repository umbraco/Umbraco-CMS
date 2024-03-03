import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Media.Tree.ReloadChildrenOf',
		name: 'Reload Media Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		meta: {
			entityTypes: [UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE],
		},
	},
];
