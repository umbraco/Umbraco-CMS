export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DeleteWithRelation',
		name: 'Delete With Relation Modal',
		element: () => import('./delete-with-relation-modal.element.js'),
	},
];
