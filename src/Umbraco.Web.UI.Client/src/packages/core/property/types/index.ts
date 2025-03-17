import type { UmbReferenceByAlias, UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export type * from './property-value-data.type.js';
export type * from './unsupported-properties.type.js';

export type UmbPropertyTypeReferenceTypeUnion = UmbReferenceByUnique | UmbReferenceByAlias;
