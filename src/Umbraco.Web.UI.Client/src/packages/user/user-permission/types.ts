export type * from './contextual-user-permission/types.js';
export type * from './entity-user-permission/types.js';
export type * from './entity-user-permission/types.js';
export type * from './granular-user-permission/types.js';
export interface UmbUserPermissionModel {
	$type: string;
	verbs: Array<string>;
}
