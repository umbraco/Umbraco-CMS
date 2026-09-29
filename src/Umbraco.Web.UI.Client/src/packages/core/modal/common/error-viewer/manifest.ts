import UmbErrorViewerModalElement from './error-viewer-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ErrorViewer',
		name: 'Error Viewer Modal',
		element: UmbErrorViewerModalElement,
	},
];
