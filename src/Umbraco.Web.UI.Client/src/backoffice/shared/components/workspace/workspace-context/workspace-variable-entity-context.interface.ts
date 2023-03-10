import type { Observable } from 'rxjs';
import { UmbVariantId } from '../../../variants/variant-id.class';
import type { UmbWorkspaceEntityContextInterface } from './workspace-entity-context.interface';
import { UmbWorkspaceSplitViewManager } from './workspace-split-view-manager.class';
import type { ValueViewModelBaseModel, VariantViewModelBaseModel } from '@umbraco-cms/backend-api';

export interface UmbWorkspaceVariableEntityContextInterface<T = unknown> extends UmbWorkspaceEntityContextInterface<T> {
	variants: Observable<Array<VariantViewModelBaseModel>>;

	splitView: UmbWorkspaceSplitViewManager;

	getName(variantId?: UmbVariantId): void;
	setName(name: string, variantId?: UmbVariantId): void;

	getVariant(variantId: UmbVariantId): VariantViewModelBaseModel | undefined;

	propertyDataByAlias(alias: string, variantId?: UmbVariantId): Observable<ValueViewModelBaseModel | undefined>;
	propertyValueByAlias(alias: string, variantId?: UmbVariantId): Observable<any | undefined>;
	getPropertyValue(alias: string, variantId?: UmbVariantId): void;
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId): void;
}
