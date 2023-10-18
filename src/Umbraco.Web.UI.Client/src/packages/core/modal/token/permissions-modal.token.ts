import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPermissionsModalData {
	unique: string;
	entityType: string;
}

export type UmbPermissionsModalValue = undefined;

export const UMB_PERMISSIONS_MODAL = new UmbModalToken<UmbPermissionsModalData, UmbPermissionsModalValue>(
	'Umb.Modal.Permissions',
	{
		type: 'sidebar',
	},
);
