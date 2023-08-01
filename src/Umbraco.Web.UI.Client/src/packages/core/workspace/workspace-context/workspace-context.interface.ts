import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbWorkspaceContextInterface<DataType = unknown> {
	workspaceAlias: string;
	repository: any; // TODO: add type
	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	setIsNew(value: boolean): void;
	getEntityId(): string | undefined; // COnsider if this should go away now that we have getUnique()
	// TODO: should we consider another name than entity type. File system files are not entities but still have this type.
	getEntityType(): string;
	getData(): DataType | undefined;
	save(): Promise<void>;
	destroy(): void;
	// TODO: temp solution to bubble validation errors to the UI
	setValidationErrors?(errorMap: any): void;
}
