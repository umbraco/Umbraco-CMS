import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestEntityAction, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_NOTIFICATIONS_MODAL_ALIAS = 'Umb.Modal.DocumentNotifications';

const actionManifest: ManifestEntityAction = {
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
};

const modalManifest: ManifestTypes = {
	type: 'modal',
	alias: UMB_DOCUMENT_NOTIFICATIONS_MODAL_ALIAS,
	name: 'Document Notifications Modal',
	js: () => import('./document-notifications-modal.element.js'),
};

export const manifests = [actionManifest, modalManifest, ...repositoryManifests];
