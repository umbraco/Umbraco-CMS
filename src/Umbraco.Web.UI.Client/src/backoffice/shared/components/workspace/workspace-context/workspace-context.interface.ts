import type { Observable } from "rxjs";

export interface UmbWorkspaceContextInterface<T = unknown> {

	readonly data: Observable<T>;

	getUnique(): string | undefined;

	getData(): T;

	load(unique: string): void;

	create(parentUnique: string | null): void;

	destroy(): void;
}
