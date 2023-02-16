export interface UmbWorkspaceContextInterface<T = unknown> {
	//readonly data: Observable<T>;
	//getUnique(): string | undefined;
	isNew: boolean;
	getEntityType(): string;
	getData(): T;
	destroy(): void;
}
