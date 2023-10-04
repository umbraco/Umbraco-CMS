import { UmbEntityData } from './entity.data.js';
import {
	DOCUMENT_ENTITY_TYPE,
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_READ,
} from '@umbraco-cms/backoffice/document';

export type UserPermissionModel = {
	id: string;
	target: unknown;
	permissions: Array<string>;
};

export const data: Array<UserPermissionModel> = [
	{
		id: '408074bb-f776-485e-b85e-c2473e45663b',
		target: {
			entityType: DOCUMENT_ENTITY_TYPE,
			documentId: 'simple-document-id',
			userGroupId: '9d24dc47-a4bf-427f-8a4a-b900f03b8a12',
		},
		permissions: [UMB_USER_PERMISSION_DOCUMENT_READ],
	},
	{
		id: 'b70b1453-a912-4157-ba62-20c2f0ab6a88',
		target: {
			entityType: DOCUMENT_ENTITY_TYPE,
			documentId: 'simple-document-id',
			userGroupId: 'f4626511-b0d7-4ab1-aebc-a87871a5dcfa',
		},
		permissions: [UMB_USER_PERMISSION_DOCUMENT_READ, UMB_USER_PERMISSION_DOCUMENT_CREATE],
	},
	{
		id: 'b70b1453-a912-4157-ba62-20c2f0ab6a88',
		target: {
			entityType: DOCUMENT_ENTITY_TYPE,
			documentId: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
			userGroupId: '9d24dc47-a4bf-427f-8a4a-b900f03b8a12',
		},
		permissions: [UMB_USER_PERMISSION_DOCUMENT_READ],
	},
];

class UmbUserPermissionData extends UmbEntityData<UserPermissionModel> {
	constructor() {
		super(data);
	}
}

export const umbUserPermissionData = new UmbUserPermissionData();
