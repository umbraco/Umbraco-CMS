import type { UmbTreeItemModelBase } from '../types.js';
import type { UmbPaginationManager } from '../../utils/pagination-manager/pagination.manager.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ProblemDetails } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbTreeItemContext<TreeItemType extends UmbTreeItemModelBase> extends UmbApi {
	unique?: string | null;
	entityType?: string;
	treeItem: Observable<TreeItemType | undefined>;
	hasChildren: Observable<boolean>;
	isLoading: Observable<boolean>;
	isSelectableContext: Observable<boolean>;
	isSelectable: Observable<boolean>;
	isSelected: Observable<boolean>;
	isActive: Observable<boolean>;
	hasActions: Observable<boolean>;
	path: Observable<string>;
	pagination: UmbPaginationManager;
	setTreeItem(treeItem: TreeItemType | undefined): void;
	requestChildren(): Promise<{
		data?: UmbPagedModel<TreeItemType> | undefined;
		error?: ProblemDetails | undefined;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;
	toggleContextMenu(): void;
	select(): void;
	deselect(): void;
	constructPath(pathname: string, entityType: string, unique: string): string;
}
