export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ContextDebugger',
		name: 'Context Debugger Modal',
		element: () => import('./modals/debug/debug-modal.element.js'),
	},
];
