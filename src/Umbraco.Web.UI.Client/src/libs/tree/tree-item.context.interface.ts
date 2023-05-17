import type { Observable } from 'rxjs';
import type { UmbControllerHostElement } from 'src/libs/controller-api';
import type { ProblemDetailsModel, TreeItemPresentationModel } from 'src/libs/backend-api';
import { UmbPagedData } from 'src/libs/repository';

export interface UmbTreeItemContext<TreeItemType extends TreeItemPresentationModel> {
	host: UmbControllerHostElement;
	unique?: string | null;
	type?: string;
	treeItem: Observable<TreeItemType | undefined>;
	hasChildren: Observable<boolean>;
	isLoading: Observable<boolean>;
	isSelectable: Observable<boolean>;
	isSelected: Observable<boolean>;
	isActive: Observable<boolean>;
	hasActions: Observable<boolean>;
	path: Observable<string>;

	setTreeItem(treeItem: TreeItemType | undefined): void;
	requestChildren(): Promise<{
		data?: UmbPagedData<TreeItemType> | undefined;
		error?: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;
	toggleContextMenu(): void;
	select(): void;
	deselect(): void;
	constructPath(pathname: string, entityType: string, unique: string): string;
}
