import type { UmbTreeItemModel, UmbTreeRootModel } from './types.js';
import type { UmbTreeContext } from './tree.context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TREE_CONTEXT = new UmbContextToken<UmbTreeContext<UmbTreeItemModel, UmbTreeRootModel>>(
	'UmbTreeContext',
);
