import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbDataTypeDetailModel = DataTypeResponseModel & {
	entityType: 'data-type';
};
