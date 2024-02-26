import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Template.QueryBuilder',
		name: 'Template query builder',
		js: () => import('./query-builder/query-builder-modal.element.js'),
	},
];

export const manifests = [...modals];
