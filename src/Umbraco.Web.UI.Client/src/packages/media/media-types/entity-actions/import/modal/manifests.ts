import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MediaType.Import',
		name: 'Media Type Import Modal',
		element: () => import('./media-type-import-modal.element.js'),
	},
];
