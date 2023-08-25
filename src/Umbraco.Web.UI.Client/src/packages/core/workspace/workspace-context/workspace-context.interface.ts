import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbWorkspaceContextInterface<DataType = unknown> {
	destroy(): void;
	workspaceAlias: string;

	save(): Promise<void>;
	// TODO: temp solution to bubble validation errors to the UI
	setValidationErrors?(errorMap: any): void;

	getEntityId(): string | undefined; // Consider if this should go away now that we have getUnique()
	// TODO: should we consider another name than entity type. File system files are not entities but still have this type.
	getEntityType(): string;

	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	setIsNew(value: boolean): void;

	/*
	// TODO: Refactor: This could maybe go away:
	repository: any; // TODO: add type
	getData(): DataType | undefined;
	*/

}
