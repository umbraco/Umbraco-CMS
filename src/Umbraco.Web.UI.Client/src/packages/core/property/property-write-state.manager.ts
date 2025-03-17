import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';
import type { UmbReferenceByAlias, UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbReferenceByVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbPropertyWriteState extends UmbState {
	propertyType: UmbReferenceByUnique | UmbReferenceByAlias | UmbReferenceByVariantId;
}

export class UmbPropertyWriteStateManager extends UmbStateManager<UmbPropertyWriteState> {}
