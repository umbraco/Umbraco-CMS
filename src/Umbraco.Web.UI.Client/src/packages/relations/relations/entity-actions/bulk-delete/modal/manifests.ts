export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.BulkDeleteWithRelation',
		name: 'Bulk Delete With Relation Modal',
		element: () => import('./bulk-delete-with-relation-modal.element.js'),
	},
];
