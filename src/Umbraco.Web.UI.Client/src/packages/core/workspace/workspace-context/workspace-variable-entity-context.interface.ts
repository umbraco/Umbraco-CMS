import type { Observable } from 'rxjs';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityWorkspaceContextInterface, UmbWorkspaceSplitViewManager } from 'src/packages/core/workspace';
import type { ValueModelBaseModel, VariantResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbWorkspaceVariableEntityContextInterface<T = unknown> extends UmbEntityWorkspaceContextInterface<T> {
	variants: Observable<Array<VariantResponseModelBaseModel>>;

	splitView: UmbWorkspaceSplitViewManager;

	getName(variantId?: UmbVariantId): void;
	setName(name: string, variantId?: UmbVariantId): void;

	getVariant(variantId: UmbVariantId): VariantResponseModelBaseModel | undefined;

	propertyDataByAlias(alias: string, variantId?: UmbVariantId): Observable<ValueModelBaseModel | undefined>;
	propertyValueByAlias(alias: string, variantId?: UmbVariantId): Observable<any | undefined>;
	getPropertyValue(alias: string, variantId?: UmbVariantId): void;
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId): void;
}
