import type { Observable } from "rxjs";
import { EntityTreeItem, PagedEntityTreeItem, ProblemDetails } from "@umbraco-cms/backend-api";

export interface UmbTreeRepository {
	requestRootItems: () => Promise<{
		data: PagedEntityTreeItem | undefined;
		error: ProblemDetails | undefined;
	}>;
	requestChildrenOf: (parentKey: string | null) => Promise<{
		data: PagedEntityTreeItem | undefined;
		error: ProblemDetails | undefined;
	}>;
	requestItems: (keys: string[]) => Promise<{
		data: Array<EntityTreeItem> | undefined;
		error: ProblemDetails | undefined;
	}>;
	rootItems: () => Promise<Observable<EntityTreeItem[]>>;
	childrenOf: (parentKey: string | null) => Promise<Observable<EntityTreeItem[]>>;
	items: (keys: string[]) => Promise<Observable<EntityTreeItem[]>>;
}
