import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MemberGroupPicker',
		name: 'Member Group Picker Modal',
		element: () => import('./member-group-picker-modal.element.js'),
	},
];
