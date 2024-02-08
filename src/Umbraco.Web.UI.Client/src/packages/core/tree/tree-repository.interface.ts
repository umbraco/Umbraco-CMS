import type { UmbTreeItemModelBase } from './types.js';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbTreeRepository<
	TreeItemType extends UmbTreeItemModelBase = UmbTreeItemModelBase,
	TreeRootType extends UmbTreeItemModelBase = UmbTreeItemModelBase,
> extends UmbApi {
	requestTreeRoot: () => Promise<{
		data?: TreeRootType;
		error?: ProblemDetails;
	}>;

	requestRootTreeItems: () => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	requestTreeItemsOf: (parentUnique: string | null) => Promise<{
		data?: UmbPagedModel<TreeItemType>;
		error?: ProblemDetails;
		asObservable?: () => Observable<TreeItemType[]>;
	}>;

	rootTreeItems: () => Promise<Observable<TreeItemType[]>>;
	treeItemsOf: (parentUnique: string | null) => Promise<Observable<TreeItemType[]>>;
}
