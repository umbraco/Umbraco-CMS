// Temp file for data type types

import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDataTypeModel extends Omit<DataTypeResponseModel, '$type'> {
	type: 'data-type' | 'data-type-folder' | 'data-type-root';
}
