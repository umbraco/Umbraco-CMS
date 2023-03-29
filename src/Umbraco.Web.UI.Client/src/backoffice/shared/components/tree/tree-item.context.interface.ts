import { Observable } from 'rxjs';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ProblemDetailsModel, TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: temp type. Add paged response type to the repository interface
interface PagedResponse<T> {
	total: number;
	items: Array<T>;
}

export interface UmbTreeItemContext<T extends TreeItemPresentationModel = TreeItemPresentationModel> {
	host: UmbControllerHostElement;
	unique?: string;
	type?: string;

	treeItem: Observable<T | undefined>;
	hasChildren: Observable<boolean>;
	isLoading: Observable<boolean>;
	isSelectable: Observable<boolean>;
	isSelected: Observable<boolean>;
	isActive: Observable<boolean>;
	hasActions: Observable<boolean>;
	path: Observable<string>;

	setTreeItem(treeItem: T | undefined): void;
	requestChildren(): Promise<{
		data: PagedResponse<T> | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<T[]>;
	}>;
	toggleContextMenu(): void;
	select(): void;
	deselect(): void;
	constructPath(pathname: string, entityType: string, unique: string): string;
}
