import { dataSet } from '../sets/index.js';
import type {
	FileSystemTreeItemPresentationModel,
	StaticFileItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockStaticFileModel = StaticFileItemResponseModel & FileSystemTreeItemPresentationModel;

export const data: Array<UmbMockStaticFileModel> = dataSet.staticFile;
