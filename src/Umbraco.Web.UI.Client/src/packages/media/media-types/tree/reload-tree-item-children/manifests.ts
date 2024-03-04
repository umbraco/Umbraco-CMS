import { UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Tree.ReloadChildrenOf',
		name: 'Reload Media Type Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE],
	},
];
