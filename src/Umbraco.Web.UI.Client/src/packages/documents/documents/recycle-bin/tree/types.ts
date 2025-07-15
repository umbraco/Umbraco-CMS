import type { UmbDocumentTreeItemModel } from '../../tree/index.js';
import type { UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentRecycleBinTreeItemModel extends Omit<UmbDocumentTreeItemModel, 'ancestors'> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentRecycleBinTreeRootModel extends UmbTreeRootModel {}
