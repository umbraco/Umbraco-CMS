import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbReferencedByAlias {
	alias: string;
}

export interface UmbMediaTypeStructureUserPermissionModel extends UmbUserPermissionModel {
	mediaType: {
		unique: string;
	};
	properties: Array<UmbReferencedByAlias>;
}
