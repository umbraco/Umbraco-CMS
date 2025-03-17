import type { UmbVariantPropertyTypeReferenceTypeUnion } from './types.js';
import { UmbPropertyReadStateManager, type UmbPropertyReadState } from '@umbraco-cms/backoffice/property';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbVariantPropertyReadState extends UmbPropertyReadState<UmbVariantPropertyTypeReferenceTypeUnion> {}

export class UmbVariantPropertyReadStateManager extends UmbPropertyReadStateManager<
	UmbVariantPropertyTypeReferenceTypeUnion,
	UmbVariantPropertyReadState
> {}
