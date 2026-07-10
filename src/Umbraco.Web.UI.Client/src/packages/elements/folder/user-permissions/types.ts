import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export type * from './conditions/types.js';

export interface UmbElementFolderUserPermissionModel extends UmbUserPermissionModel {
	// TODO: this should be unique instead of an id, but we currently have no way to map a mixed server response.
	elementContainer: { id: string };
}
