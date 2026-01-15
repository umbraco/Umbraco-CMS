export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.BulkTrashWithRelation',
		name: 'Bulk Trash With Relation Modal',
		element: () => import('./bulk-trash-with-relation-modal.element.js'),
	},
];
