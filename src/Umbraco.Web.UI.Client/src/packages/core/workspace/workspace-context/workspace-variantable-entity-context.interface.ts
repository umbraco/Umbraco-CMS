import type { UmbWorkspaceSplitViewManager } from '../workspace-split-view-manager.class.js';
import type { UmbDatasetContext } from '../dataset-context/dataset-context.interface.js';
import type { UmbSaveableWorkspaceContextInterface } from './saveable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { ValueModelBaseModel, VariantResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbVariantableWorkspaceContextInterface<EntityType = unknown> extends UmbSaveableWorkspaceContextInterface<EntityType> {
	variants: Observable<Array<VariantResponseModelBaseModel>>;

	splitView: UmbWorkspaceSplitViewManager;

	getName(variantId?: UmbVariantId): void;
	setName(name: string, variantId?: UmbVariantId): void;

	getVariant(variantId: UmbVariantId): VariantResponseModelBaseModel | undefined;

	//propertyDataByAlias(alias: string, variantId?: UmbVariantId): Observable<ValueModelBaseModel | undefined>;

	// This one is async cause it needs to structure to provide this data:
	propertyDataById(id: string): Promise<Observable<ValueModelBaseModel | undefined>>;
	propertyValueByAlias(alias: string, variantId?: UmbVariantId): Observable<any | undefined>;
	getPropertyValue(alias: string, variantId?: UmbVariantId): void;
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId): void;

	// Dataset methods:
	createVariableDatasetContext(host: UmbControllerHost, variantId: UmbVariantId): UmbDatasetContext;
}
