import UmbWorkspaceModalElement from './workspace-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Workspace',
		name: 'Workspace Modal',
		element: UmbWorkspaceModalElement,
	},
];
