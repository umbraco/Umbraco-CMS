import { html, nothing } from '@umbraco-cms/backoffice/external/lit';

// Inline, as the separator is often rendered inside another element's shadow root, which the host's styles do not reach.
const SEPARATOR_STYLE =
	'border-top: 1px solid var(--uui-color-divider-standalone); margin: var(--uui-size-space-2) var(--uui-size-space-3);';

/**
 * Renders a separator before an item in a list of grouped extensions, when its group differs from the previous item's.
 * Items without a group form a group of their own. Nothing is rendered before the first item.
 * @param {{ group?: string } | undefined} previous - The manifest of the previous item, or `undefined` for the first item.
 * @param {{ group?: string }} current - The manifest of the current item.
 * @param {string} [current.group] - The group of the current item.
 * @returns {unknown} A separator, or `nothing`.
 */
export function umbRenderGroupSeparator(previous: { group?: string } | undefined, current: { group?: string }) {
	if (!previous || previous.group === current.group) return nothing;
	return html`<div role="separator" style=${SEPARATOR_STYLE}></div>`;
}
