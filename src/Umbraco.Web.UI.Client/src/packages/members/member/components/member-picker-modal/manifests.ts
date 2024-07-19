import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MemberPicker',
		name: 'Member Picker Modal',
		element: () => import('./member-picker-modal.element.js'),
	},
];
