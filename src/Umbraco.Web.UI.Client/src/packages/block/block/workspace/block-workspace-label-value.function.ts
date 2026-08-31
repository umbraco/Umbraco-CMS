import type { UmbBlockDataValueModel, UmbBlockLabelUfmValueType } from '../types.js';

/**
 * Builds the value object consumed by the block label UFM render. Content values are placed at the top
 * level and settings values under a `$settings` key — both keyed by property alias — so labels can
 * reference them as `${alias}` and `${$settings.alias}` respectively.
 * @param {Array<UmbBlockDataValueModel> | undefined} contentValues - The resolved block content values.
 * @param {Array<UmbBlockDataValueModel> | undefined} settingsValues - The resolved block settings values.
 * @param {number | undefined} index - The block index, exposed as `$index` when defined.
 * @returns {UmbBlockLabelUfmValueType} The value object for the label render.
 */
export function buildBlockLabelValueObject(
	contentValues: Array<UmbBlockDataValueModel> | undefined,
	settingsValues: Array<UmbBlockDataValueModel> | undefined,
	index?: number,
): UmbBlockLabelUfmValueType {
	const valueObject: UmbBlockLabelUfmValueType = {};

	if (contentValues) {
		for (const property of contentValues) {
			valueObject[property.alias] = property.value;
		}
	}

	if (settingsValues) {
		const settingsObject: Record<string, unknown> = {};
		for (const property of settingsValues) {
			settingsObject[property.alias] = property.value;
		}
		valueObject['$settings'] = settingsObject;
	}

	if (index !== undefined) {
		valueObject['$index'] = index;
	}

	return valueObject;
}
