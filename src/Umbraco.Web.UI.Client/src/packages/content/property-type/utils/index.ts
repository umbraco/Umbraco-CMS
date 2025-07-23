import type { UmbPropertyTypeData } from '../types.js';

/**
 * Validation Data Path Query generator for Property Type.
 * write a JSON-Path filter similar to `?(@.id == '1234-1224-1244')`
 * @param {UmbPropertyTypeData} value - the object holding Property Type.
 * @returns {string} - a JSON-path query
 */
export function UmbDataPathPropertyTypeQuery(value: Pick<UmbPropertyTypeData, 'id'>): string {
	return `?(@.id == '${value.id}')`;
}
