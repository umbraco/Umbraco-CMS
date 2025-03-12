import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbReferencedByAlias {
	alias: string;
}

export interface UmbDocumentValueUserPermissionModel extends UmbUserPermissionModel {
	documentType: {
		unique: string;
	};
	property: UmbReferencedByAlias;
}
