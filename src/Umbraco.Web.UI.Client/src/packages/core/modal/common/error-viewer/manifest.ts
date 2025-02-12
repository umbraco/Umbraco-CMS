import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ErrorViewer',
		name: 'Error Viewer Modal',
		element: () => import('./error-viewer-modal.element.js'),
	},
];
