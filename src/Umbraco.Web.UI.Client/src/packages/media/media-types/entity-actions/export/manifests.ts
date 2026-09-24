import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ACTION_GROUP_IMPORT_EXPORT } from '@umbraco-cms/backoffice/action';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MediaType.Export',
		group: UMB_ACTION_GROUP_IMPORT_EXPORT,
		name: 'Export Media Type Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		api: () => import('./media-type-export.action.js'),
		meta: {
			icon: 'icon-download-alt',
			label: '#actions_export',
			additionalOptions: true,
		},
	},
	...repositoryManifests,
];
