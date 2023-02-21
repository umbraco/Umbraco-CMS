import type { Observable } from 'rxjs';
import type { UmbWorkspaceEntityContextInterface } from './workspace-entity-context.interface';
import type { ValueViewModelBaseModel } from '@umbraco-cms/backend-api';

export interface UmbWorkspaceInvariantableEntityContextInterface<T = unknown>
	extends UmbWorkspaceEntityContextInterface<T> {
	getName(): void;
	setName(name: string): void;

	propertyDataByAlias(alias: string): Observable<ValueViewModelBaseModel | undefined>;
	propertyValueByAlias(alias: string): Observable<any | undefined>;
	getPropertyValue(alias: string): void;
	setPropertyValue(alias: string, value: unknown): void;
}
