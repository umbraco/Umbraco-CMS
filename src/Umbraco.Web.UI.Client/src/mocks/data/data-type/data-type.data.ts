import { dataSet } from '../sets/index.js';
import type {
	DataTypeItemResponseModel,
	DataTypeResponseModel,
	DataTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockDataTypeModel = DataTypeResponseModel & DataTypeTreeItemResponseModel & DataTypeItemResponseModel;

export const data: Array<UmbMockDataTypeModel> = dataSet.dataType;
