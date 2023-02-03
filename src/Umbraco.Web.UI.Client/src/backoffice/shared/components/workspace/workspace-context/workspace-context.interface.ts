import type { Observable } from "rxjs";

export interface UmbWorkspaceContextInterface<T = unknown> {

	readonly data: Observable<T>;


	//getUnique(): string | undefined;

	getData(): T;

	destroy(): void;
}
