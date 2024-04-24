import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Template.QueryBuilder',
		name: 'Template query builder',
		element: () => import('./query-builder/query-builder-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...modals];
