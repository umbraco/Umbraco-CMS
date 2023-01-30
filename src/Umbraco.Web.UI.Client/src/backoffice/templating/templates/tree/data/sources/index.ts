import { DataSourceResponse } from '../../../workspace/data';
import { EntityTreeItem, PagedEntityTreeItem } from '@umbraco-cms/backend-api';

export interface TemplateTreeDataSource {
	getTreeRoot(): Promise<DataSourceResponse<PagedEntityTreeItem>>;
	getTreeItemChildren(parentKey: string): Promise<DataSourceResponse<PagedEntityTreeItem>>;
	getTreeItems(key: Array<string>): Promise<DataSourceResponse<EntityTreeItem[]>>;
}
