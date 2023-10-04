import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbEntityUserPermissionSettingsModalData {
	unique: string;
	entityType: string;
}

export type UmbEntityUserPermissionSettingsModalValue = undefined;

export const UMB_ENTITY_USER_PERMISSION_MODAL = new UmbModalToken<
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue
>('Umb.Modal.EntityUserPermissionSettings', {
	type: 'sidebar',
});
