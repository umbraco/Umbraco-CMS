import type { ManifestPropertyEditorUi } from '../extensions/types.js';

/**
 * Narrows a set of Property Editor UIs to those the picker offers.
 *
 * Deprecated ones are left out, so they cannot be chosen for a data type that is not already using one. They do
 * keep working where they are in use, and one stays in the set while it is the current selection: without that, a
 * data type using a deprecated editor would open the picker showing nothing selected.
 * @param {Array<ManifestPropertyEditorUi>} propertyEditorUis The Property Editor UIs to narrow.
 * @param {Array<string>} selectedAliases The aliases currently selected.
 * @param {boolean} showDeprecated Whether the deprecated ones are offered anyway.
 * @returns {Array<ManifestPropertyEditorUi>} The Property Editor UIs to offer.
 */
export function selectablePropertyEditorUis(
	propertyEditorUis: Array<ManifestPropertyEditorUi>,
	selectedAliases: Array<string>,
	showDeprecated = false,
): Array<ManifestPropertyEditorUi> {
	if (showDeprecated) {
		return propertyEditorUis;
	}

	return propertyEditorUis.filter(
		(propertyEditorUi) => !propertyEditorUi.meta.deprecated || selectedAliases.includes(propertyEditorUi.alias),
	);
}
