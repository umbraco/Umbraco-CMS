import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentPublish';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
	name: 'Document Publish Modal',
	element: () => import('./document-publish-modal.element.js'),
};
