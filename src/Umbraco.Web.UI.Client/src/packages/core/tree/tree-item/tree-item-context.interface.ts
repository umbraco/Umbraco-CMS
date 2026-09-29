import type { UmbTreeItemApi } from '../tree-item-api/tree-item-api.interface.js';
import type { UmbTreeItemModel } from '../types.js';
import type { UmbPaginationManager, UmbTargetPaginationManager } from '@umbraco-cms/backoffice/utils';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbTreeItemContext<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
> extends UmbTreeItemApi<TreeItemType> {
	childItems: Observable<TreeItemType[]>;
	hasChildren: Observable<boolean>;
	isLoading: Observable<boolean>;
	isOpen: Observable<boolean>;
	pagination: UmbPaginationManager;
	targetPagination: UmbTargetPaginationManager;
	isLoadingPrevChildren: Observable<boolean>;
	isLoadingNextChildren: Observable<boolean>;
	loadChildren(): void;
	reloadChildren(): void;
	showChildren(): void;
	hideChildren(): void;
	loadPrevItems(): void;
	loadNextItems(): void;
	setIsMenu(isMenu: boolean): void;
	getIsMenu(): boolean;
}
