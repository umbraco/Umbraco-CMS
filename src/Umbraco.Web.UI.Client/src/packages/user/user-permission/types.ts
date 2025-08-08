export type * from './user-granular-permission.extension.js';
export type * from './entity-user-permission.extension.js';
export type * from './user-extension-permission.extension.js';

export interface UmbUserPermissionModel {
	$type: string;
	userPermissionType?: string;
	verbs: Array<string>;
}
