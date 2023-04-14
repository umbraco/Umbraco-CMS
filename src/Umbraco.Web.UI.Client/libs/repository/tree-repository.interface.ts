import type { Observable } from 'rxjs';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbPagedData<T> {
	total: number;
	items: Array<T>;
}

export interface UmbTreeRepository<ItemType = any, PagedItemType = UmbPagedData<ItemType>> {
	requestRootTreeItems: () => Promise<{
		data: PagedItemType | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<ItemType[]>;
	}>;
	requestTreeItemsOf: (parentUnique: string | null) => Promise<{
		data: PagedItemType | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<ItemType[]>;
	}>;
	requestTreeItems: (uniques: string[]) => Promise<{
		data: Array<ItemType> | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<ItemType[]>;
	}>;

	rootTreeItems: () => Promise<Observable<ItemType[]>>;
	treeItemsOf: (parentUnique: string | null) => Promise<Observable<ItemType[]>>;
	treeItems: (uniques: string[]) => Promise<Observable<ItemType[]>>;
}
