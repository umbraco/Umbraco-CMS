import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MemberType.Import',
		name: 'Member Type Import Modal',
		element: () => import('./member-type-import-modal.element.js'),
	},
];
