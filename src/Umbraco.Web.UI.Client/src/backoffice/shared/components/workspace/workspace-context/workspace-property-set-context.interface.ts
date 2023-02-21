import { Observable } from 'rxjs';
import { ValueViewModelBaseModel } from '@umbraco-cms/backend-api';

export interface UmbWorkspacePropertySetContextInterface {
	propertyDataByAlias(alias: string): Observable<ValueViewModelBaseModel | undefined>;
	propertyValueByAlias(alias: string): Observable<any | undefined>;
	getPropertyValue(alias: string): void;
	setPropertyValue(alias: string, value: unknown): void;
}
