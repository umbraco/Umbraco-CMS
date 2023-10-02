import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPermissionsModalData {
	unique: string;
	entityType: string;
}

export type UmbPermissionsModalResult = undefined;

export const UMB_PERMISSIONS_MODAL = new UmbModalToken<UmbPermissionsModalData, UmbPermissionsModalResult>(
	'Umb.Modal.Permissions',
	{
		type: 'sidebar',
	},
);
