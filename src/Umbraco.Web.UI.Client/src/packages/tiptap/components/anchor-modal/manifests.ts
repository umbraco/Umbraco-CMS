import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Anchor',
		name: 'Anchor Modal',
		element: () => import('./anchor-modal.element.js'),
	},
];
