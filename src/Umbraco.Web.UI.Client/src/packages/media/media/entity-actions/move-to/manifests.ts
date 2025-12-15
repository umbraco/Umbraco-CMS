import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Media.MoveTo',
		name: 'Move Media Entity Action',
		kind: 'default',
		api: () => import('./move-media.action.js'),
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
		meta: {
			icon: 'icon-enter',
			label: '#actions_move',
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	...repositoryManifests,
];
