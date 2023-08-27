import type { UmbDatasetContext } from '../dataset-context/dataset-context.interface.js';
import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ValueModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbInvariableWorkspaceContextInterface<T = unknown>
	extends UmbSaveableWorkspaceContextInterface<T> {

	// Name:
	getName(): string | undefined;
	setName(name: string): void;

	// Property:
	propertyDataById(id: string): Observable<ValueModelBaseModel | undefined>;
	propertyValueByAlias<ReturnType = unknown>(alias: string): Observable<ReturnType | undefined>;
	getPropertyValue(alias: string): void;
	setPropertyValue(alias: string, value: unknown): void;

	// Dataset methods:
	createDatasetContext(host: UmbControllerHost): UmbDatasetContext;
}
