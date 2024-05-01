import type { UmbUserGroupEntityType } from './entity.js';

export interface UmbUserGroupDetailModel {
	alias: string;
	documentRootAccess: boolean;
	documentStartNode: { unique: string } | null;
	entityType: UmbUserGroupEntityType;
	fallbackPermissions: Array<string>;
	hasAccessToAllLanguages: boolean;
	icon: string | null;
	isSystemGroup: boolean;
	languages: Array<string>;
	mediaRootAccess: boolean;
	mediaStartNode: { unique: string } | null;
	name: string;
	// TODO: add type
	permissions: Array<any>;
	sections: Array<string>;
	unique: string;
}
