import { dataSet } from './sets/index.js';
import type {
	FileSystemTreeItemPresentationModel,
	StylesheetItemResponseModel,
	StylesheetResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockStylesheetModel = StylesheetResponseModel &
	FileSystemTreeItemPresentationModel &
	StylesheetItemResponseModel;

export const data: Array<UmbMockStylesheetModel> = dataSet.stylesheet;
