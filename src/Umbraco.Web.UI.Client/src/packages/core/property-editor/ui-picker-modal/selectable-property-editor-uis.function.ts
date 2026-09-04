import type { ManifestPropertyEditorUi } from '../extensions/types.js';

/**
 * Narrows a set of Property Editor UIs to those that can be picked for a data type.
 *
 * A deprecated Property Editor UI keeps working for the data types already using it, so it is still offered
 * while it is the current selection — otherwise the only way to reopen the picker would be to lose it.
 * @param {Array<ManifestPropertyEditorUi>} propertyEditorUis The Property Editor UIs to narrow.
 * @param {Array<string>} selectedAliases The aliases currently selected.
 * @returns {Array<ManifestPropertyEditorUi>} The Property Editor UIs that can be picked.
 */
export function umbSelectablePropertyEditorUis(
	propertyEditorUis: Array<ManifestPropertyEditorUi>,
	selectedAliases: Array<string>,
): Array<ManifestPropertyEditorUi> {
	return propertyEditorUis.filter(
		(propertyEditorUi) => !propertyEditorUi.meta.deprecated || selectedAliases.includes(propertyEditorUi.alias),
	);
}
