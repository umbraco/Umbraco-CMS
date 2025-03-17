import type { UmbVariantPropertyTypeReferenceTypeUnion } from './types.js';
import { UmbPropertyWriteStateManager, type UmbPropertyWriteState } from '@umbraco-cms/backoffice/property';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbVariantPropertyWriteState extends UmbPropertyWriteState<UmbVariantPropertyTypeReferenceTypeUnion> {}

export class UmbVariantPropertyWriteStateManager extends UmbPropertyWriteStateManager<
	UmbVariantPropertyTypeReferenceTypeUnion,
	UmbVariantPropertyWriteState
> {}
