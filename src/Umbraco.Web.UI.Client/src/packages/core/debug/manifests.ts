import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ContextDebugger',
		name: 'Context Debugger Modal',
		element: () => import('./modals/debug/debug-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...modals];
