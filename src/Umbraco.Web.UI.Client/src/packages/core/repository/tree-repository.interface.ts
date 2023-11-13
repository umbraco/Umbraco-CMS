import { type UmbPagedData } from './data-source/types.js';
import { type Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbTreeRootEntityModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { ProblemDetails, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbTreeRepository<
	TreeItemType extends EntityTreeItemResponseModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootEntityModel,
> extends UmbApi {
	requestTreeRoot: () => Promise<{
		data?: TreeRootType;
		error?: ProblemDetails;
	}>;

	requestTreeItemsOf: (parentUnique: string | null) => Promise<{
		data?: UmbPagedData<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	treeItemsOf: (parentUnique: string | null) => Promise<Observable<TreeItemType[]>>;

	/* TODO: remove this. It is not used client side.
	Logic to call the root endpoint should be in the data source 
	because it is a server decision to split them
	*/
	requestRootTreeItems: () => Promise<{
		data?: UmbPagedData<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	// TODO: remove
	rootTreeItems: () => Promise<Observable<TreeItemType[]>>;

	// TODO: remove this when all repositories are migrated to the new interface items interface
	requestItemsLegacy?: (uniques: string[]) => Promise<{
		data?: Array<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<any[]>;
	}>;

	// TODO: remove this when all repositories are migrated to the new items interface
	itemsLegacy?: (uniques: string[]) => Promise<Observable<any[]>>;
}
