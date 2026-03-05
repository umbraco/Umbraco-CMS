import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';
export type * from './conditions/types.js';
export interface UmbElementUserPermissionModel extends UmbUserPermissionModel {
	// TODO: this should be unique instead of an id, but we currently have no way to map a mixed server response.
	element: { id: string };
}
