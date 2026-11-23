/**
 * An option of a value list, holding the value stored when it is selected and the label shown for it.
 */
export interface UmbValueListOption {
	value: string;
	label: string;
}

/**
 * Reads the configured items of a value list into options.
 * An item is either a plain string, used as the value, or an object holding a `value` and a `label` (or a `name`).
 * A plain string is both the value and the label. The label of an object without one is empty. Items without a string value are skipped.
 * @param {unknown} items - The configured items.
 * @returns {Array<UmbValueListOption>} The options, in the order of the items.
 */
export function umbParseValueListOptions(items: unknown): Array<UmbValueListOption> {
	if (!Array.isArray(items)) return [];

	const options: Array<UmbValueListOption> = [];
	for (const item of items) {
		if (typeof item === 'string') {
			options.push({ value: item, label: item });
			continue;
		}

		if (item && typeof item === 'object' && typeof item.value === 'string') {
			const label = item.label || item.name;
			options.push({ value: item.value, label: typeof label === 'string' ? label : '' });
		}
	}

	return options;
}
