import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import { UMB_ENTITY_ACTION_GROUP_IMPORT_EXPORT } from '@umbraco-cms/backoffice/entity-action';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MemberType.Import',
		group: UMB_ENTITY_ACTION_GROUP_IMPORT_EXPORT,
		weight: 500,
		name: 'Import Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
		api: () => import('./member-type-import.action.js'),
		meta: {
			icon: 'icon-page-up',
			label: '#actions_import',
			additionalOptions: true,
		},
	},
	...repositoryManifests,
	...modalManifests,
];
