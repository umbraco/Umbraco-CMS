import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS = 'Umb.Modal.DocumentPublishWithDescendants';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS,
	name: 'Document Publish With Descendants Modal',
	element: () => import('./document-publish-with-descendants-modal.element.js'),
};
