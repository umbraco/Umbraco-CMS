import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const actionManifests: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Notifications',
		name: 'Notifications',
		api: () => import('./document-notifications.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-megaphone',
			label: '#notifications_notifications',
		},
	},
];

export const manifests = [...actionManifests, ...modalManifests, ...repositoryManifests];
