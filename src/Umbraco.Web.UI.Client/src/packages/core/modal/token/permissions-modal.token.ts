import { UmbModalToken } from './modal-token.js';

export interface UmbPermissionsModalData {
	unique: string;
	entityType: string;
}

export type UmbPermissionsModalValue = undefined;

export const UMB_PERMISSIONS_MODAL = new UmbModalToken<UmbPermissionsModalData, UmbPermissionsModalValue>(
	'Umb.Modal.Permissions',
	{
		modal: {
			type: 'sidebar',
		},
	},
);
