import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ValueModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbInvariableWorkspaceContextInterface<T = unknown>
	extends UmbSaveableWorkspaceContextInterface<T> {
	getName(): void;
	setName(name: string): void;

	propertyDataById(id: string): Observable<ValueModelBaseModel | undefined>;
	propertyValueByAlias(alias: string): Observable<any | undefined>;
	getPropertyValue(alias: string): void;
	setPropertyValue(alias: string, value: unknown): void;
}
