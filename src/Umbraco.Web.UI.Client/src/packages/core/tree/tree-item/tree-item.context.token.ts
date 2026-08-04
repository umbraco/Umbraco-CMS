import type { UmbTreeItemApi } from '../tree-item-api/tree-item-api.interface.js';
import type { UmbTreeItemContext } from './tree-item-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TREE_ITEM_CONTEXT = new UmbContextToken<UmbTreeItemApi>('UmbTreeItemContext');

export const UMB_TREE_ITEM_EXPANDABLE_CONTEXT = new UmbContextToken<UmbTreeItemApi, UmbTreeItemContext>(
	'UmbTreeItemContext',
	undefined,
	(context): context is UmbTreeItemContext => 'loadChildren' in context,
);
