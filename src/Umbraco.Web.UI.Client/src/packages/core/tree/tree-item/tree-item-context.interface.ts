import type { UmbTreeItemApi } from './tree-item-base/tree-item-api-base.js';
import type { UmbTreeItemModel } from '../types.js';
import type { UmbPaginationManager } from '../../utils/pagination-manager/pagination.manager.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbTargetPaginationManager } from '@umbraco-cms/backoffice/utils';

export interface UmbTreeItemContext<TreeItemType extends UmbTreeItemModel = UmbTreeItemModel>
	extends UmbTreeItemApi<TreeItemType> {
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
