export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Workspace',
		name: 'Workspace Modal',
		js: () => import('./workspace-modal.element.js'),
	},
];
