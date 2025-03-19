import type { UmbReferenceByVariantId } from './types.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbPropertyReadStateManager, type UmbPropertyReadState } from '@umbraco-cms/backoffice/property';

export interface UmbVariantPropertyReadState extends UmbPropertyReadState {
	propertyType: UmbReferenceByUnique & UmbReferenceByVariantId;
}

export class UmbVariantPropertyReadStateManager extends UmbPropertyReadStateManager<UmbVariantPropertyReadState> {}
