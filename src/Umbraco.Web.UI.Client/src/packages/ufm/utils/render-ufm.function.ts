import type { UmbUfmResolvedEvent } from '../components/ufm-render/ufm-render.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import '../components/ufm-render/ufm-render.element.js';

/**
 * Renders a `<umb-ufm-render>` projected into a `name` slot, resolving markdown to plain text
 * and emitting `umb-ufm-resolved` for a listener to pick up (e.g. to populate a block context's
 * resolved name for `<umb-entity-frame>`).
 * @param {string | undefined} markdown the UFM markdown to resolve.
 * @param {unknown} value the value context passed to the UFM render.
 * @param {(event: UmbUfmResolvedEvent) => void} onResolved called when the UFM render emits its resolved text.
 * @returns {unknown} a Lit template for the UFM render, slotted as `name`.
 */
export function renderUfm(
	markdown: string | undefined,
	value: unknown,
	onResolved: (event: UmbUfmResolvedEvent) => void,
) {
	return html`
		<umb-ufm-render slot="name" inline .markdown=${markdown} .value=${value} @umb-ufm-resolved=${onResolved}>
		</umb-ufm-render>
	`;
}
