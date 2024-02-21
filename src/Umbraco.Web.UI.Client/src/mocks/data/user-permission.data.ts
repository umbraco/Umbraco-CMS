import { UmbEntityData } from './entity.data.js';
import type {
	DocumentPermissionModel,
	FallbackPermissionModel,
	UnknownTypePermissionModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<FallbackPermissionModel | DocumentPermissionModel | UnknownTypePermissionModel> = [
	{
		verbs: ['Umb.Document.Read'],
		document: {
			id: 'simple-document-id',
		},
	},
];

class UmbUserPermissionData extends UmbEntityData<any> {
	constructor() {
		super(data);
	}
}

export const umbUserPermissionData = new UmbUserPermissionData();
