import { UmbEntityData } from './entity.data.js';

export interface UserPermissionModel {
	id: string;
	verb: string;
}

export interface DocumentUserPermissionModel extends UserPermissionModel {
	document?: { id: string };
}

export const data: Array<UserPermissionModel | DocumentUserPermissionModel> = [
	{
		id: '408074bb-f776-485e-b85e-c2473e45663b',
		verb: 'Umb.Document.Read',
		document: {
			id: 'simple-document-id',
		},
	},
];

class UmbUserPermissionData extends UmbEntityData<UserPermissionModel> {
	constructor() {
		super(data);
	}
}

export const umbUserPermissionData = new UmbUserPermissionData();
