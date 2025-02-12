import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: 'Umb.Modal.ErrorViewer',
	name: 'Error Viewer Modal',
	element: () => import('./error-viewer-modal.element.js'),
};
