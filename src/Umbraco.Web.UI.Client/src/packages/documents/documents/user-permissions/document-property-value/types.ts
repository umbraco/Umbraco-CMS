import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbDocumentPropertyValueUserPermissionModel extends UmbUserPermissionModel {
	documentType: {
		unique: string;
	};
	propertyType: {
		unique: string;
	};
}
