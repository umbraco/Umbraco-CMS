import type { UmbUfmResolvedEvent } from '../components/ufm-render/ufm-render.element.js';
import { html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

import '../components/ufm-render/ufm-render.element.js';

/**
 * Renders a visible `<umb-ufm-render>` that resolves markdown to plain text and emits
 * `umb-ufm-resolved` for a listener to pick up.
 * @param {string | undefined} markdown the UFM markdown to resolve.
 * @param {unknown} value the value context passed to the UFM render.
 * @param {(event: UmbUfmResolvedEvent) => void} onResolved called when the UFM render emits its resolved text.
 * @param {string} [slot] the slot of the host element to project the render into.
 * @returns {unknown} a Lit template for the UFM render.
 */
export function renderUfm(
	markdown: string | undefined,
	value: unknown,
	onResolved: (event: UmbUfmResolvedEvent) => void,
	slot?: string,
) {
	return html`
		<umb-ufm-render
			slot=${ifDefined(slot)}
			inline
			.markdown=${markdown}
			.value=${value}
			@umb-ufm-resolved=${onResolved}>
		</umb-ufm-render>
	`;
}
