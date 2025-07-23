import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbEntityUserPermissionSettingsModalData {
	entityType: string;
	/**
	 * Unique identifier for the entity
	 * @deprecated The unique is not used in the modal as it is not needed. It is kept for backwards compatibility. Will be removed in v17.
	 * @type {string}
	 * @memberof UmbEntityUserPermissionSettingsModalData
	 */
	unique?: string;
	headline?: string;
	preset?: UmbEntityUserPermissionSettingsModalValue;
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
