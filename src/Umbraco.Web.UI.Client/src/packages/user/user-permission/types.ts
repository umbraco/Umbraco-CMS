export type * from './user-granular-permission.extension.js';
export type * from './entity-user-permission.extension.js';
export type * from './contextual-user-permission/types.js';
export interface UmbUserPermissionModel {
	$type: string;
	verbs: Array<string>;
}
