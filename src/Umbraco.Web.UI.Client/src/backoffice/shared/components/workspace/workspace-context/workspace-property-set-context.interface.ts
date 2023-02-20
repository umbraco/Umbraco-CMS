import { Observable } from 'rxjs';
import { PropertyViewModelBaseModel } from '@umbraco-cms/backend-api';

export interface UmbWorkspacePropertySetContextInterface {
	propertyInfoByAlias(alias: string): Observable<PropertyViewModelBaseModel | undefined>;
	propertyValueByAlias(alias: string): Observable<any | undefined>;
	getPropertyValue(alias: string): void;
	setPropertyValue(alias: string, value: unknown): void;
}
