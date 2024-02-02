export interface UserGroupDetailModel {
	unique: string;
	entityType: UmbUserGroupEntityType;
	isSystemGroup: boolean;
	name: string;
	icon: string | null;
	sections: Array<string>;
	languages: Array<string>;
	hasAccessToAllLanguages: boolean;
	documentStartNode: { unique: string } | null;
	documentRootAccess: boolean;
	mediaStartNode: { unique: string } | null;
	mediaRootAccess: boolean;
	permissions: Array<string>;
}
