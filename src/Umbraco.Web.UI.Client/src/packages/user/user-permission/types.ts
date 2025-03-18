export type * from './user-granular-permission.extension.js';
export type * from './entity-user-permission.extension.js';
export interface UmbUserPermissionModel {
	$type: string;
	permissionType: string;
	verbs: Array<string>;
}
