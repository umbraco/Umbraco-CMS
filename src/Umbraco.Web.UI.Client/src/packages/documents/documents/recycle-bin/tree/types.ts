import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentRecycleBinTreeItemModel extends Omit<UmbEntityTreeItemModel, 'name'> {}

export interface UmbDocumentRecycleBinTreeRootModel extends UmbEntityTreeRootModel {}
