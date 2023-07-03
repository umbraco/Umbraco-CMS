import { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MODAL_TEMPLATING_QUERY_BUILDER_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Query.Builder.Sidebar';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_QUERY_BUILDER_SIDEBAR_ALIAS,
		name: 'Template query builder',
		loader: () => import('./query-builder/query-builder.element.js'),
	},
];

export const manifests = [...modals];
