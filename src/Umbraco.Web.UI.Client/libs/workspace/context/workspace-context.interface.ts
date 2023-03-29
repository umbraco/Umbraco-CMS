import { Observable } from 'rxjs';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export interface UmbWorkspaceContextInterface<DataType = unknown> {
	host: UmbControllerHostElement;
	repository: any; // TODO: add type
	isNew: Observable<boolean>;
	getIsNew(): boolean;
	setIsNew(value: boolean): void;
	// TODO: should we consider another name than entity type. File system files are not entities but still have this type.
	getEntityType(): string;
	getData(): DataType | undefined;
	destroy(): void;
	// TODO: temp solution to bubble validation errors to the UI
	setValidationErrors?(errorMap: any): void;
}
