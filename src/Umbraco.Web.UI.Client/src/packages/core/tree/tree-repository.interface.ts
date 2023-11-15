import type { UmbTreeItemModelBase, UmbTreeItemModel, UmbEntityTreeRootModel } from './types.js';
import type { UmbPagedData } from '@umbraco-cms/backoffice/repository';
import { type Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbTreeRepository<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeItemModelBase = UmbEntityTreeRootModel,
> extends UmbApi {
	requestTreeRoot: () => Promise<{
		data?: TreeRootType;
		error?: ProblemDetails;
	}>;

	requestRootTreeItems: () => Promise<{
		data?: UmbPagedData<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	requestTreeItemsOf: (parentUnique: string | null) => Promise<{
		data?: UmbPagedData<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	// TODO: remove this when all repositories are migrated to the new interface items interface
	requestItemsLegacy?: (uniques: string[]) => Promise<{
		data?: Array<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<any[]>;
	}>;

	rootTreeItems: () => Promise<Observable<TreeItemType[]>>;

	treeItemsOf: (parentUnique: string | null) => Promise<Observable<TreeItemType[]>>;

	// TODO: remove this when all repositories are migrated to the new items interface
	itemsLegacy?: (uniques: string[]) => Promise<Observable<any[]>>;
}
