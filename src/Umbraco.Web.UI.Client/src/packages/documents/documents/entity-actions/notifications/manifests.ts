import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS } from '../../user-permissions/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

const actionManifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Notifications',
		name: 'Notifications',
		weight: 100,
		api: () => import('./document-notifications.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-megaphone',
			label: '#actions_notify',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];

export const manifests = [...actionManifests, ...modalManifests, ...repositoryManifests];
