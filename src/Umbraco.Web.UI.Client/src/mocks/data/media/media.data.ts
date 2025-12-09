import { dataSet } from '../sets/index.js';
import type {
	MediaItemResponseModel,
	MediaResponseModel,
	MediaTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMediaModel = MediaResponseModel & MediaTreeItemResponseModel & MediaItemResponseModel;

export const data: Array<UmbMockMediaModel> = dataSet.media;
