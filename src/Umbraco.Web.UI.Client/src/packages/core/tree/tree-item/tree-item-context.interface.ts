import type { UmbTreeItemModel } from '../types.js';
import type { UmbPaginationManager } from '../../utils/pagination-manager/pagination.manager.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbContextMinimal } from '@umbraco-cms/backoffice/context-api';
import type { UmbTargetPaginationManager } from '@umbraco-cms/backoffice/utils';

export interface UmbTreeItemContext<TreeItemType extends UmbTreeItemModel = UmbTreeItemModel>
	extends UmbApi,
		UmbContextMinimal {
	unique?: string | null;
	entityType?: string;
	treeItem: Observable<TreeItemType | undefined>;
	childItems: Observable<TreeItemType[]>;
	hasChildren: Observable<boolean>;
	isLoading: Observable<boolean>;
	isSelectableContext: Observable<boolean>;
	isSelectable: Observable<boolean>;
	isSelected: Observable<boolean>;
	isActive: Observable<boolean>;
	isOpen: Observable<boolean>;
	hasActions: Observable<boolean>;
	path: Observable<string>;
	pagination: UmbPaginationManager;
	targetPagination: UmbTargetPaginationManager;
	getTreeItem(): TreeItemType | undefined;
	setTreeItem(treeItem: TreeItemType | undefined): void;
	select(): void;
	deselect(): void;
	constructPath(pathname: string, entityType: string, unique: string): string;
	loadChildren(): void;
	reloadChildren(): void;
	showChildren(): void;
	hideChildren(): void;
	loadPrevItems(): void;
	loadNextItems(): void;
	isLoadingPrevChildren: Observable<boolean>;
	isLoadingNextChildren: Observable<boolean>;
}
