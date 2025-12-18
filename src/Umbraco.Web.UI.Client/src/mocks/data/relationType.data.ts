import { dataSet } from './sets/index.js';
import type {
	RelationTypeItemResponseModel,
	RelationTypeResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockRelationTypeModel = RelationTypeResponseModel & RelationTypeItemResponseModel;
export type UmbMockRelationTypeItemModel = RelationTypeItemResponseModel;

export const data: Array<UmbMockRelationTypeModel> = dataSet.relationType;
