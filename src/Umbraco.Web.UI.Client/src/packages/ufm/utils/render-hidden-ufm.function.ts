import type { UmbUfmResolvedEvent } from '../components/ufm-render/ufm-render.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import '../components/ufm-render/ufm-render.element.js';

/**
 * Renders an off-screen `<umb-ufm-render>` whose only purpose is to resolve markdown
 * to plain text and emit `umb-ufm-resolved`. Use for paths that have no visible UFM
 * render but still need to populate downstream state (e.g. a block context's resolved
 * name for `<umb-entity-frame>`).
 *
 * Uses inline styles because the wrapper may be rendered inside a shadow DOM (e.g.
 * `<umb-extension-slot>`) where scoped styles can't reach.
 * @param {string | undefined} markdown the UFM markdown to resolve.
 * @param {unknown} value the value context passed to the UFM render.
 * @param {(event: UmbUfmResolvedEvent) => void} onResolved called when the UFM render emits its resolved text.
 * @returns {unknown} a Lit template for the hidden UFM render.
 */
export function renderHiddenUfm(
	markdown: string | undefined,
	value: unknown,
	onResolved: (event: UmbUfmResolvedEvent) => void,
) {
	return html`
		<div style="position:absolute;inset:0;visibility:hidden;pointer-events:none;overflow:hidden;">
			<umb-ufm-render inline .markdown=${markdown} .value=${value} @umb-ufm-resolved=${onResolved}></umb-ufm-render>
		</div>
	`;
}
