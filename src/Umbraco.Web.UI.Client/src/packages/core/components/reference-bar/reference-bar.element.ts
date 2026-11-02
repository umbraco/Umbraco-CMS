import { css, customElement, html, LitElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * An overlay that marks its parent as a reference. It draws a bar along the parent's left edge
 * with a link icon tab attached to it. The overlay ignores pointer events, so it does not
 * interfere with the content beneath it.
 * The parent must establish a positioning context (e.g. `position: relative`).
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
			}

			.border {
				position: absolute;
				width: 2px;
				height: 100%;
				background-color: var(--umb-color-reference);
			}

			.tab {
				position: fixed;
				top: var(--uui-size-6);
				background-color: var(--umb-color-reference);
				color: var(--umb-color-reference-contrast);
				padding: var(--uui-size-2) var(--uui-size-1) var(--uui-size-2) var(--uui-size-2);
				border-radius: var(--uui-border-radius) 0 0 var(--uui-border-radius);
				font-size: var(--uui-type-small-size);
				line-height: 0;
				margin-left: -22px;
			}

			@supports (position-anchor: --my-name) {
				:host {
					anchor-name: --umb-tab-anchor;
				}

				.tab {
					position-anchor: --umb-tab-anchor;
					right: anchor(right);
					margin-left: auto;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-reference-bar': UmbReferenceBarElement;
	}
}
