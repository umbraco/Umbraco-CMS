import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CultureAndHostnames',
		name: 'Culture And Hostnames Modal',
		js: () => import('./culture-and-hostnames/culture-and-hostnames-modal.element.js'),
	},
];

export const manifests = [...modals];
