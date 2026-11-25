import { css, customElement, html, property, LitElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * An overlay that frames its parent with a rounded border and shows a label tab
 * just above the parent's top-right corner. Visibility is controlled by the consumer via
 * `--umb-entity-frame-opacity` (defaults to `1`); the typical pattern is for the parent
 * container to set it to `0` by default and toggle to `1` on `:hover` and/or `:focus-within`.
 * The parent must establish a positioning context (e.g. `position: relative`), and must not
 * clip overflow above its top edge (the tab renders outside the parent's content box).
 * @element umb-entity-frame
 * @slot - Optional rich content for the tab. Falls back to the `label` property.
 * @cssprop --umb-entity-frame-border-width - Thickness of the border. Defaults to `1px`.
 * @cssprop --umb-entity-frame-color - Accent colour for the border and tab background. Defaults to `--uui-color-focus`.
 * @cssprop --umb-entity-frame-contrast-color - Text colour for the tab. Defaults to `--uui-color-selected-contrast`. Override when using a custom `--umb-entity-frame-color` that does not pair with the default contrast token.
 * @cssprop --umb-entity-frame-offset - Distance the border is inset from the parent's edge. Defaults to `0px`. A negative value moves the frame (and its tab) outwards, e.g. `-1px` places it one pixel further out.
 * @cssprop --umb-entity-frame-opacity - Opacity of the border and tab. Defaults to `1`. Set to `0` on the parent and toggle to `1` on `:hover` / `:focus-within` to gate visibility.
 * @augments {LitElement}
 */
@customElement('umb-entity-frame')
export class UmbEntityFrameElement extends LitElement {
	/**
	 * Text displayed in the tab when no slot content is projected.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	label: string = '';

	override render() {
		return html`
			<div class="border" aria-hidden="true"></div>
			<div class="tab"><slot>${this.label}</slot></div>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				position: absolute;
				inset: 0;
				pointer-events: none;
				z-index: 1;
			}

			.border,
			.tab {
				opacity: var(--umb-entity-frame-opacity, 1);
				transition: opacity 120ms ease-out;
			}

			.border {
				position: absolute;
				inset: var(--umb-entity-frame-offset, 0px);
				border: var(--umb-entity-frame-border-width, 1px) solid var(--umb-entity-frame-color, var(--uui-color-focus));
				border-radius: var(--uui-border-radius);
				box-sizing: border-box;
				pointer-events: none;
			}

			.tab {
				position: absolute;
				display: flex;
				align-items: center;
				gap: var(--uui-size-1);
				min-height: calc(var(--uui-size-7) - var(--umb-entity-frame-border-width, 1px));
				bottom: calc(100% - var(--umb-entity-frame-offset, 0px));
				right: var(--umb-entity-frame-offset, 0px);
				background: var(--umb-entity-frame-color, var(--uui-color-focus));
				color: var(--umb-entity-frame-contrast-color, var(--uui-color-selected-contrast));
				padding: calc(var(--umb-entity-frame-border-width, 1px) + 1px) var(--uui-size-2) 0;
				border-radius: var(--uui-border-radius) var(--uui-border-radius) 0 0;
				font-size: var(--uui-type-small-size);
				line-height: 1.125;
			}

			.tab::before {
				content: '';
				position: absolute;
				background-color: transparent;
				bottom: calc((var(--uui-border-radius) * -2) - var(--umb-entity-frame-border-width, 1px));
				height: calc(var(--uui-border-radius) * 2);
				width: var(--uui-border-radius);
				right: 0;
				z-index: -1;
				border-top-right-radius: var(--uui-border-radius);
				box-shadow: 0 calc(var(--uui-border-radius) * -1) 0 0 var(--umb-entity-frame-color, var(--uui-color-focus));
			}

			::slotted(uui-icon) {
				margin-top: -0.125em;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-frame': UmbEntityFrameElement;
	}
}
