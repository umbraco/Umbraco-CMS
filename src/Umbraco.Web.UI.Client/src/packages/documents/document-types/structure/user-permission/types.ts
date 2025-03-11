import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbReferencedByAlias {
	alias: string;
}

export interface UmbDocumentTypeStructureUserPermissionModel extends UmbUserPermissionModel {
	documentType: {
		unique: string;
	};
	properties: Array<UmbReferencedByAlias>;
}
