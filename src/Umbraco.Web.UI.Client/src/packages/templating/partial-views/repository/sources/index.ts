import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface PartialViewsTreeDataSource {
	getRootItems(): Promise<DataSourceResponse<PagedFileSystemTreeItemPresentationModel>>;
	getChildrenOf({
		path,
		skip,
		take,
	}: {
		path?: string | undefined;
		skip?: number | undefined;
		take?: number | undefined;
	}): Promise<DataSourceResponse<PagedFileSystemTreeItemPresentationModel>>;
	getItem(ids: Array<string>): Promise<DataSourceResponse<FileSystemTreeItemPresentationModel[]>>;
}
