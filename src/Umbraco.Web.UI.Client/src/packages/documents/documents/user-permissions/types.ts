import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbDocumentUserPermissionModel extends UmbUserPermissionModel {
	document: { unique: string };
}
