import type { Observable } from 'rxjs';
import { EntityTreeItemResponseModel, PagedEntityTreeItemResponseModel, ProblemDetailsModel } from '@umbraco-cms/backend-api';

export interface UmbTreeRepository {
	requestRootTreeItems: () => Promise<{
		data: PagedEntityTreeItemResponseModel | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<EntityTreeItemResponseModel[]>;
	}>;
	requestTreeItemsOf: (parentKey: string | null) => Promise<{
		data: PagedEntityTreeItemResponseModel | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<EntityTreeItemResponseModel[]>;
	}>;
	requestTreeItems: (keys: string[]) => Promise<{
		data: Array<EntityTreeItemResponseModel> | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<EntityTreeItemResponseModel[]>;
	}>;

	rootTreeItems: () => Promise<Observable<EntityTreeItemResponseModel[]>>;
	treeItemsOf: (parentKey: string | null) => Promise<Observable<EntityTreeItemResponseModel[]>>;
	treeItems: (keys: string[]) => Promise<Observable<EntityTreeItemResponseModel[]>>;
}
