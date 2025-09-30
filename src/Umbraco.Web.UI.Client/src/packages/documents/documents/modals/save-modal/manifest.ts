import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_SAVE_MODAL_ALIAS = 'Umb.Modal.DocumentSave';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: UMB_DOCUMENT_SAVE_MODAL_ALIAS,
	name: 'Document Save Modal',
	element: () => import('./document-save-modal.element.js'),
};
