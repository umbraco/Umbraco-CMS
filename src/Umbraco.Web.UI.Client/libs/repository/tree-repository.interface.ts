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

	// TODO: remove this when all repositories are migrated to the new interface items interface
	requestItemsLegacy?: (uniques: string[]) => Promise<{
		data: Array<ItemType> | undefined;
		error: ProblemDetailsModel | undefined;
		asObservable?: () => Observable<ItemType[]>;
	}>;

	rootTreeItems: () => Promise<Observable<ItemType[]>>;
	treeItemsOf: (parentUnique: string | null) => Promise<Observable<ItemType[]>>;

	// TODO: remove this when all repositories are migrated to the new items interface
	itemsLegacy?: (uniques: string[]) => Promise<Observable<ItemType[]>>;
}
