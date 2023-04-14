import type { Observable } from 'rxjs';
import type { UmbEntityWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type { ValueModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbWorkspaceInvariantableEntityContextInterface<T = unknown>
	extends UmbEntityWorkspaceContextInterface<T> {
	getName(): void;
	setName(name: string): void;

	propertyDataByAlias(alias: string): Observable<ValueModelBaseModel | undefined>;
	propertyValueByAlias(alias: string): Observable<any | undefined>;
	getPropertyValue(alias: string): void;
	setPropertyValue(alias: string, value: unknown): void;
}
