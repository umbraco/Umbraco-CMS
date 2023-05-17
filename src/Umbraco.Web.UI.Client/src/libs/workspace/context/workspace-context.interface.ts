import { Observable } from 'rxjs';
import { UmbControllerHostElement } from 'src/libs/controller-api';

export interface UmbWorkspaceContextInterface<DataType = unknown> {
	host: UmbControllerHostElement;
	repository: any; // TODO: add type
	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	setIsNew(value: boolean): void;
	// TODO: should we consider another name than entity type. File system files are not entities but still have this type.
	getEntityType(): string;
	getData(): DataType | undefined;
	save(): Promise<void>;
	destroy(): void;
	// TODO: temp solution to bubble validation errors to the UI
	setValidationErrors?(errorMap: any): void;
}
