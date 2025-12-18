import { dataSet } from './sets/index.js';
import type {
	FileSystemTreeItemPresentationModel,
	PartialViewItemResponseModel,
	PartialViewResponseModel,
	PartialViewSnippetResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockPartialViewModel = PartialViewResponseModel &
	FileSystemTreeItemPresentationModel &
	PartialViewItemResponseModel;

export const data: Array<UmbMockPartialViewModel> = dataSet.partialView;

export const snippets: Array<PartialViewSnippetResponseModel> = dataSet.partialViewSnippets;
