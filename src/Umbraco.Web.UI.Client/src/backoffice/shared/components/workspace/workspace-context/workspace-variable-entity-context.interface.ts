import type { Observable } from 'rxjs';
import { UmbVariantId } from '../../../variants/variant-id.class';
import type { UmbWorkspaceEntityContextInterface } from './workspace-entity-context.interface';
import type { ValueViewModelBaseModel } from '@umbraco-cms/backend-api';

export interface UmbWorkspaceVariableEntityContextInterface<T = unknown> extends UmbWorkspaceEntityContextInterface<T> {
	getName(variantId?: UmbVariantId): void;
	setName(name: string, variantId?: UmbVariantId): void;

	propertyDataByAlias(alias: string, variantId?: UmbVariantId): Observable<ValueViewModelBaseModel | undefined>;
	propertyValueByAlias(alias: string, variantId?: UmbVariantId): Observable<any | undefined>;
	getPropertyValue(alias: string, variantId?: UmbVariantId): void;
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId): void;
}
