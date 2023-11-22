import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbDataTypeDetailModel = Omit<DataTypeResponseModel, 'id' | 'parentId'> & {
	entityType: string;
	unique: string | undefined; // TODO - remove this when server doesn't allow undefined
	parentUnique: string | null | undefined; // TODO - remove this when server doesn't allow undefined
};
