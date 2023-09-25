import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbEntityUserPermissionSettingsModalData {
	unique: string;
	entityType: Array<string>;
}

export type UmbEntityUserPermissionSettingsModalResult = undefined;

export const UMB_ENTITY_USER_PERMISSION_MODAL = new UmbModalToken<
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalResult
>('Umb.Modal.EntityUserPermissionSettings', {
	type: 'sidebar',
});
