export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.TrashWithRelation',
		name: 'Trash With Relation Modal',
		element: () => import('./trash-with-relation-modal.element.js'),
	},
];
