export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Workspace',
		name: 'Workspace Modal',
		element: () => import('./workspace-modal.element.js'),
	},
];
