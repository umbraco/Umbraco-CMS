import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: 'Umb.Modal.ManifestViewer',
	name: 'Manifest Viewer Modal',
	element: () => import('./manifest-viewer-modal.element.js'),
};
