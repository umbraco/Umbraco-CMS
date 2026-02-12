import type { UmbTreeItemContext } from './tree-item-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TREE_ITEM_CONTEXT = new UmbContextToken<UmbTreeItemContext>('UmbTreeItemContext');
