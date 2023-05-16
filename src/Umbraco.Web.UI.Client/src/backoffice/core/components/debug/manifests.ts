import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ContextDebugger',
		name: 'Context Debugger Modal',
		loader: () => import('./modals/debug/debug-modal.element'),
	},
];

export const manifests = [...modals];
