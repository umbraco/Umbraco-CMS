import { dataSet } from './sets/index.js';
import type { RelationResponseModel, RelationReferenceModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockRelationModel = RelationResponseModel;
export type UmbMockRelationReferenceModel = RelationReferenceModel;

export const data: Array<UmbMockRelationModel> = dataSet.relation;
