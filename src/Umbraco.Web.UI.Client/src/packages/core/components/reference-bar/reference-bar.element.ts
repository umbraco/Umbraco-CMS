import { css, customElement, html, LitElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * An overlay that frames its parent with a rounded border and shows a label tab
 * just above the parent's top-right corner. Visibility is controlled by the consumer via
 * `--umb-reference-bar-opacity` (defaults to `1`); the typical pattern is for the parent
 * container to set it to `0` by default and toggle to `1` on `:hover` and/or `:focus-within`.
 * The parent must establish a positioning context (e.g. `position: relative`), and must not
 * clip overflow above its top edge (the tab renders outside the parent's content box).
 * @element umb-reference-bar
 * @augments {LitElement}
 */
@customElement('umb-reference-bar')
export class UmbReferenceBarElement extends LitElement {
	override render() {
		return html`
			<div class="border" aria-hidden="true"></div>
			<div class="tab"><uui-icon name="link"></uui-icon></div>
		`;
	}

	// TODO: Remove position-anchor hack for the tab once the uui-modal-sidebar supports elements sticking out of its container. [NL]
	static override readonly styles = [
		css`
			:host {
				position: absolute;
				top: 0;
				bottom: 0;
				left: 0;
				pointer-events: none;

				z-index: 2;
				anchor-name: --umb-tab-anchor;
			}

			.border {
				position: absolute;
				width: 2px;
				height: 100%;
				background-color: var(--umb-color-reference);
			}

			.tab {
				position: fixed;
				position-anchor: --umb-tab-anchor;
				top: anchor(top);
				right: anchor(right);
				position-anchor: top right;

				top: var(--uui-size-6);
				background-color: var(--umb-color-reference);
				color: var(--umb-color-reference-contrast);
				padding: var(--uui-size-2) var(--uui-size-1) var(--uui-size-2) var(--uui-size-2);
				border-radius: var(--uui-border-radius) 0 0 var(--uui-border-radius);
				font-size: var(--uui-type-small-size);
				line-height: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-reference-bar': UmbReferenceBarElement;
	}
}
