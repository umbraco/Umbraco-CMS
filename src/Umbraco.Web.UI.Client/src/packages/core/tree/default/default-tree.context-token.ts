import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import type { UmbDefaultTreeContext } from './default-tree.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TREE_CONTEXT = new UmbContextToken<UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel>>(
	'UmbTreeContext',
);
