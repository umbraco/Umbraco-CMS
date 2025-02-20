export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Template.QueryBuilder',
		name: 'Template query builder',
		element: () => import('./query-builder/query-builder-modal.element.js'),
	},
];
