import type { Observable } from "rxjs";

export interface UmbWorkspaceContextInterface<T = unknown> {

	readonly data: Observable<T>;


	//getUnique(): string | undefined;

	getEntityType(): string;

	getData(): T;

	destroy(): void;
}
