import type { UmbPropertyValueData } from '../../property/types.js';

/**
 * @function UmbObjectToPropertyValueArray
 * @param {object} data - an object with properties to be converted.
 * @returns {Array<UmbPropertyValueData> | undefined} - and array of property values or undefined
 */
export function umbObjectToPropertyValueArray(data: object | undefined): Array<UmbPropertyValueData> | undefined {
	if (!data) return;
	return Object.keys(data).map((key) => ({
		alias: key,
		value: (data as any)[key],
	}));
}
