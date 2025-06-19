export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CodeEditor',
		name: 'Code Editor Modal',
		element: () => import('./code-editor-modal.element.js'),
	},
];
