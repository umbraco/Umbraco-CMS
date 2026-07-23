import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../workspace/member/member-workspace.context-token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-member-profile-data-workspace-info-app')
export class UmbMemberProfileDataWorkspaceInfoAppElement extends UmbLitElement {
	@state()
	private _profileData?: string | null;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (context) => {
			this.observe(context?.profileData, (data) => (this._profileData = data));
		});
	}

	#formatKey(key: string): string {
		// Convert camelCase / PascalCase / snake_case / kebab-case into "Title Case".
		// Common IdP claim shapes: givenName, family_name, preferred-username, Email, etc.
		return key
			.replace(/[_-]+/g, ' ')
			.replace(/([a-z])([A-Z])/g, '$1 $2')
			.replace(/\s+/g, ' ')
			.trim()
			.replace(/\b\w/g, (c) => c.toUpperCase());
	}

	#renderValue(value: unknown, depth = 0): unknown {
		if (value === null || value === undefined) {
			return html`<em>${this.localize.term('general_none')}</em>`;
		}
		if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
			return String(value);
		}
		if (Array.isArray(value)) {
			// Arrays of primitives read well as a comma-separated list;
			// anything richer falls back to JSON so the structure stays visible.
			const allPrimitive = value.every(
				(v) =>
					v === null ||
					v === undefined ||
					typeof v === 'string' ||
					typeof v === 'number' ||
					typeof v === 'boolean',
			);
			if (allPrimitive) {
				return value.map((v) => (v === null || v === undefined ? '—' : String(v))).join(', ');
			}
			return html`<pre>${JSON.stringify(value, null, 2)}</pre>`;
		}
		// Nested object — recurse one level, then fall back to JSON for anything deeper
		// so the panel stays readable on pathological input.
		if (depth >= 1) {
			return html`<pre>${JSON.stringify(value, null, 2)}</pre>`;
		}
		const entries = Object.entries(value as Record<string, unknown>);
		if (entries.length === 0) {
			return html`<em>${this.localize.term('general_none')}</em>`;
		}
		return html`
			<div class="nested">
				${entries.map(
					([key, childValue]) => html`
						<div class="nested-entry">
							<span class="nested-key">${this.#formatKey(key)}</span>
							<span class="nested-value">${this.#renderValue(childValue, depth + 1)}</span>
						</div>
					`,
				)}
			</div>
		`;
	}

	#renderContent() {
		if (!this._profileData) return nothing;

		let parsed: unknown;
		try {
			parsed = JSON.parse(this._profileData);
		} catch {
			// Not valid JSON — fall back to the raw string.
			return html`<pre>${this._profileData}</pre>`;
		}

		// Top-level object: render each property as a label/value row — label left, value right —
		// matching the existing Username / Email / Member Group layout via <umb-property-layout>.
		// Anything else (primitive, array, top-level array) falls back to pretty-printed JSON.
		if (parsed !== null && typeof parsed === 'object' && !Array.isArray(parsed)) {
			const entries = Object.entries(parsed as Record<string, unknown>);
			if (entries.length === 0) return nothing;
			return html`
				${entries.map(
					([key, value]) => html`
						<umb-property-layout label=${this.#formatKey(key)}>
							<div slot="editor">${this.#renderValue(value)}</div>
						</umb-property-layout>
					`,
				)}
			`;
		}

		return html`<pre>${JSON.stringify(parsed, null, 2)}</pre>`;
	}

	override render() {
		// Only render the box for members that actually have profile data (i.e. external members
		// whose integrator populated it). Content members and external members without a profile
		// payload skip the box entirely.
		if (!this._profileData) return nothing;

		return html`
			<umb-workspace-info-app-layout headline="#member_profileData">
				${this.#renderContent()}
			</umb-workspace-info-app-layout>
		`;
	}

	static override styles = [
		css`
			/*
			 * umb-workspace-info-app-layout zeroes --uui-box-default-padding so sibling apps can
			 * paint edge-to-edge. Reintroduce the inset on our rows so label + value align with
			 * the Username / Email / Member Group box above.
			 */
			umb-property-layout {
				padding: var(--uui-size-space-4) var(--uui-size-layout-1);
			}
			umb-property-layout:first-of-type {
				padding-top: var(--uui-size-space-5);
			}
			umb-property-layout:last-of-type {
				padding-bottom: var(--uui-size-space-5);
			}

			pre {
				margin: var(--uui-size-space-3) var(--uui-size-layout-1);
				padding: var(--uui-size-space-3) var(--uui-size-space-5);
				background: var(--uui-color-surface-alt);
				border-radius: var(--uui-border-radius, 3px);
				white-space: pre-wrap;
				word-break: break-word;
				font-family: var(--uui-font-family-mono, monospace);
				font-size: var(--uui-type-small-size, 0.75rem);
			}

			.nested {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
				padding-left: var(--uui-size-space-4);
				border-left: 2px solid var(--uui-color-divider);
			}
			.nested-entry {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: baseline;
			}
			.nested-key {
				min-width: 120px;
				flex-shrink: 0;
				font-weight: 600;
				color: var(--uui-color-text-alt);
			}
			.nested-value {
				word-break: break-word;
			}
			/* Pretty-printed fallback inside a nested slot shouldn't carry the top-level box inset. */
			.nested-value pre {
				margin: 0;
			}
		`,
	];
}

export default UmbMemberProfileDataWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-profile-data-workspace-info-app': UmbMemberProfileDataWorkspaceInfoAppElement;
	}
}
