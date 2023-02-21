import { Observable } from 'rxjs';

export interface UmbWorkspaceContextInterface<T = unknown> {
	//readonly data: Observable<T>;
	//getUnique(): string | undefined;
	isNew: Observable<T>;
	getIsNew(): boolean;
	setIsNew(value: boolean): void;
	getEntityType(): string;
	getData(): T;
	destroy(): void;
}
