import type { Observable } from 'rxjs';
import { EntityTreeItemModel, PagedEntityTreeItemModel, ProblemDetailsModel } from '@umbraco-cms/backend-api';

export interface UmbTreeRepository {
	requestRootTreeItems: () => Promise<{
		data: PagedEntityTreeItemModel | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<EntityTreeItemModel[]>;
	}>;
	requestTreeItemsOf: (parentKey: string | null) => Promise<{
		data: PagedEntityTreeItemModel | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<EntityTreeItemModel[]>;
	}>;
	requestTreeItems: (keys: string[]) => Promise<{
		data: Array<EntityTreeItemModel> | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<EntityTreeItemModel[]>;
	}>;

	rootTreeItems: () => Promise<Observable<EntityTreeItemModel[]>>;
	treeItemsOf: (parentKey: string | null) => Promise<Observable<EntityTreeItemModel[]>>;
	treeItems: (keys: string[]) => Promise<Observable<EntityTreeItemModel[]>>;
}
