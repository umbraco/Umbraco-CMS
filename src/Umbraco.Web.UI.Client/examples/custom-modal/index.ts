import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const demoModals: Array<ManifestModal> = [
	{
		type: 'modal',
		name: 'Example Custom Modal Element',
		alias: 'example.modal.custom.element',
		js: () => import('./example-modal-view.element.js'),
	},
];
export default [...demoModals]; 
