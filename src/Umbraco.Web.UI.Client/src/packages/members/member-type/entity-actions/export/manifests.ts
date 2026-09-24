import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ACTION_GROUP_IMPORT_EXPORT } from '@umbraco-cms/backoffice/action';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MemberType.Export',
		group: UMB_ACTION_GROUP_IMPORT_EXPORT,
		name: 'Export Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
		api: () => import('./member-type-export.action.js'),
		meta: {
			icon: 'icon-download-alt',
			label: '#actions_export',
			additionalOptions: true,
		},
	},
	...repositoryManifests,
];
