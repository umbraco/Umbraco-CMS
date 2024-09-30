import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_NOTIFICATIONS_MODAL_ALIAS = 'Umb.Modal.DocumentNotifications';

const modalManifest: ManifestModal = {
	type: 'modal',
	alias: UMB_DOCUMENT_NOTIFICATIONS_MODAL_ALIAS,
	name: 'Document Notifications Modal',
	element: () => import('./document-notifications-modal.element.js'),
};

export const manifests = [modalManifest];
