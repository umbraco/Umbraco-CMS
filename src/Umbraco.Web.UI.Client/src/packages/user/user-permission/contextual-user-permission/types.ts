import type { UmbUserPermissionModel } from '../types.js';

export type * from './user-permission.extension.js';

export interface UmbContextualUserPermissionModel extends UmbUserPermissionModel {
	$type: 'UnknownTypePermissionPresentationModel';
	context: string;
}
