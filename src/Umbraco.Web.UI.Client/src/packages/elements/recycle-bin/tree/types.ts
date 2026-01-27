import type { UmbElementTreeItemModel } from '../../tree/index.js';
import type { UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementRecycleBinTreeItemModel extends Omit<UmbElementTreeItemModel, 'ancestors'> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementRecycleBinTreeRootModel extends UmbTreeRootModel {}
