import UmbContextDebuggerModalElement from './debug-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ContextDebugger',
		name: 'Context Debugger Modal',
		element: UmbContextDebuggerModalElement,
	},
];
