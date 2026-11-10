import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_ACTION_GROUP_IMPORT_EXPORT } from '@umbraco-cms/backoffice/entity-action';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MemberType.Export',
		group: UMB_ENTITY_ACTION_GROUP_IMPORT_EXPORT,
		weight: 500,
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
