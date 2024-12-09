import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentUnpublish';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS,
	name: 'Document Unpublish Modal',
	element: () => import('./document-unpublish-modal.element.js'),
};
