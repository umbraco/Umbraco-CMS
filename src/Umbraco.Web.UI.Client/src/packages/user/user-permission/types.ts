export type * from './user-granular-permission.extension.js';
export type * from './entity-user-permission.extension.js';
export type * from './user-permission/types.js';
export interface UmbUserPermissionModel {
	$type: string;
	verbs: Array<string>;
}

export interface UmbContextualUserPermissionModel extends UmbUserPermissionModel {
	$type: 'UnknownTypePermissionPresentationModel';
	context: string;
}
