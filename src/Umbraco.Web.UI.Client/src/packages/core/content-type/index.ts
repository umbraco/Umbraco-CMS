export * from './constants.js';
export * from './repository/index.js';
export * from './structure/index.js';
export * from './workspace/index.js';

export type * from './composition/index.js';
export type * from './types.js';

/**
 * @deprecated Use `UmbPropertyTypeBasedPropertyElement` from `@umbraco-cms/backoffice/content` instead.
 * Export will be removed in version 17.
 */
export { UmbPropertyTypeBasedPropertyElement } from '../content/components/property-type-based-property/property-type-based-property.element.js';
