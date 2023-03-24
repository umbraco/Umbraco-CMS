import type { Observable } from 'rxjs';
import {
	EntityTreeItemResponseModel,
	PagedEntityTreeItemResponseModel,
	ProblemDetailsModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbTreeRepository<
	PagedItemsType = PagedEntityTreeItemResponseModel,
	ItemsType = EntityTreeItemResponseModel
> {
	requestRootTreeItems: () => Promise<{
		data: PagedItemsType | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<ItemsType[]>;
	}>;
	requestTreeItemsOf: (parentUnique: string | null) => Promise<{
		data: PagedItemsType | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<ItemsType[]>;
	}>;
	requestTreeItems: (uniques: string[]) => Promise<{
		data: Array<ItemsType> | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<ItemsType[]>;
	}>;

	rootTreeItems: () => Promise<Observable<ItemsType[]>>;
	treeItemsOf: (parentUnique: string | null) => Promise<Observable<ItemsType[]>>;
	treeItems: (uniques: string[]) => Promise<Observable<ItemsType[]>>;
}
