import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbDocumentValueUserPermissionModel extends UmbUserPermissionModel {
	documentType: {
		unique: string;
	};
	property: {
		unique: string;
	};
}
