import { css, customElement, html, LitElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * An overlay that marks its parent as a reference. It draws a bar along the parent's left edge
 * with a link icon tab attached to it. The overlay ignores pointer events, so it does not
 * interfere with the content beneath it.
 * The parent must establish a positioning context (e.g. `position: relative`).
 * @element umb-entity-bar
 * @augments {LitElement}
 */
@customElement('umb-entity-bar')
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
				left: -1px;
				pointer-events: none;

				opacity: var(--umb-entity-bar-opacity, 1);
				transition: opacity 120ms ease-out;
			}

			.border {
				position: absolute;
				width: 1px;
				height: 100%;
				background-color: var(--umb-color-reference);
			}

			.tab {
				position: absolute;
				top: var(--uui-size-6);
				background-color: var(--umb-color-reference);
				color: var(--umb-color-reference-contrast);
				padding: var(--uui-size-2) var(--uui-size-1) var(--uui-size-2) var(--uui-size-2);
				border-radius: var(--uui-border-radius) 0 0 var(--uui-border-radius);
				font-size: var(--uui-type-small-size);
				line-height: 0;
				right: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-bar': UmbReferenceBarElement;
	}
}
