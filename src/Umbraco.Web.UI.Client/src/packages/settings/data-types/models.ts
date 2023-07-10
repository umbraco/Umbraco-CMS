import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDataTypeModel extends DataTypeResponseModel {
	type: 'data-type' | 'data-type-folder' | 'data-type-root';
}
