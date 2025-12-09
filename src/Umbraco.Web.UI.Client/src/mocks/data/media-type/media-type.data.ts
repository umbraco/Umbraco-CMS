import { dataSet } from '../sets/index.js';
import type {
	MediaTypeItemResponseModel,
	MediaTypeResponseModel,
	MediaTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMediaTypeModel = MediaTypeResponseModel &
	MediaTypeTreeItemResponseModel &
	MediaTypeItemResponseModel;

export type UmbMockMediaTypeUnionModel =
	| MediaTypeResponseModel
	| MediaTypeTreeItemResponseModel
	| MediaTypeItemResponseModel;

export const data: Array<UmbMockMediaTypeModel> = dataSet.mediaType;
