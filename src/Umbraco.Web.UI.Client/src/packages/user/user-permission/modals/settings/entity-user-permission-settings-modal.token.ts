import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbEntityUserPermissionSettingsModalData {
	unique: string;
	entityType: string;
	headline?: string;
}

export type UmbEntityUserPermissionSettingsModalValue = {
	allowedVerbs: Array<string>;
};

export const UMB_ENTITY_USER_PERMISSION_MODAL = new UmbModalToken<
	UmbEntityUserPermissionSettingsModalData,
	UmbEntityUserPermissionSettingsModalValue
>('Umb.Modal.EntityUserPermissionSettings', {
	modal: {
		type: 'sidebar',
	},
});
