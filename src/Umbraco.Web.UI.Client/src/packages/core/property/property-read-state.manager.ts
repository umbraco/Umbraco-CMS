import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';
import type { UmbReferenceByAlias, UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbReferenceByVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbPropertyReadState extends UmbState {
	propertyType: UmbReferenceByUnique | UmbReferenceByAlias | UmbReferenceByVariantId;
}

export class UmbPropertyReadStateManager extends UmbStateManager<UmbPropertyReadState> {}
