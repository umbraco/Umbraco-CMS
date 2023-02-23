import { Observable } from 'rxjs';

export interface UmbWorkspaceContextInterface<T = unknown> {
	isNew: Observable<boolean>;
	getIsNew(): boolean;
	setIsNew(value: boolean): void;
	getEntityType(): string;
	getData(): T;
	destroy(): void;
	// TODO: temp solution to bubble validation errors to the UI
	setValidationErrors?(errorMap: any): void;
}
