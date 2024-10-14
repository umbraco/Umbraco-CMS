import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';
export type * from './conditions/types.js';
export interface UmbDocumentUserPermissionModel extends UmbUserPermissionModel {
	// TODO: this should be unique instead of an id, but we currently have now way to map a mixed server response.
	document: { id: string };
}
