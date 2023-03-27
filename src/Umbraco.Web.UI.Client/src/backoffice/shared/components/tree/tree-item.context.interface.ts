import { Observable } from 'rxjs';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostInterface } from 'libs/controller/controller-host.mixin';

// TODO: temp type. Add paged response type to the repository interface
interface PagedResponse<T> {
	total: number;
	items: Array<T>;
}

export interface UmbTreeItemContext<T> {
	host: UmbControllerHostInterface;
	treeItem: T;
	unique: string;
	type: string;

	hasChildren: Observable<boolean>;
	isLoading: Observable<boolean>;
	isSelectable: Observable<boolean>;
	isSelected: Observable<boolean>;
	isActive: Observable<boolean>;
	hasActions: Observable<boolean>;
	path: Observable<string>;

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
