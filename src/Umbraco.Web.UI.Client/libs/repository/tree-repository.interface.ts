import type { Observable } from "rxjs";
import { EntityTreeItem, PagedEntityTreeItem, ProblemDetails } from "@umbraco-cms/backend-api";

export interface UmbTreeRepository {

	requestRootTreeItems: () => Promise<{
		data: PagedEntityTreeItem | undefined;
		error: ProblemDetails | undefined;
		asObservable?: () => Observable<EntityTreeItem[]>;
	}>;
	requestTreeItemsOf: (parentKey: string | null) => Promise<{
		data: PagedEntityTreeItem | undefined;
		error: ProblemDetails | undefined;
		asObservable?: () => Observable<EntityTreeItem[]>;
	}>;
	requestTreeItems: (keys: string[]) => Promise<{
		data: Array<EntityTreeItem> | undefined;
		error: ProblemDetails | undefined;
		asObservable?: () => Observable<EntityTreeItem[]>;
	}>;

	rootTreeItems: () => Promise<Observable<EntityTreeItem[]>>;
	treeItemsOf: (parentKey: string | null) => Promise<Observable<EntityTreeItem[]>>;
	treeItems: (keys: string[]) => Promise<Observable<EntityTreeItem[]>>;
}
