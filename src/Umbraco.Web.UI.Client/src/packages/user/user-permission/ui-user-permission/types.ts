import type { UmbUserPermissionModel } from '../types.js';

export type * from './ui-user-permission.extension.js';

export interface UmbUiUserPermissionModel extends UmbUserPermissionModel {
	$type: 'UnknownTypePermissionPresentationModel';
	context: string;
}
