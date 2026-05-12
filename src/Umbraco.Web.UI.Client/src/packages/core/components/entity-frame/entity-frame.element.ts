import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, LitElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * A passive overlay that frames its parent with a rounded border and shows a label tab
 * just above the parent's top-right corner. Visibility is controlled by the consumer via
 * `--umb-entity-frame-opacity` (defaults to `1`); the typical pattern is for the parent
 * container to set it to `0` by default and toggle to `1` on `:hover` and/or `:focus-within`.
 * The parent must establish a positioning context (e.g. `position: relative`), and must not
 * clip overflow above its top edge (the tab renders outside the parent's content box).
 * @element umb-entity-frame
 * @slot - Optional rich content for the tab. Falls back to the `label` property.
 * @cssprop --umb-entity-frame-border-width - Thickness of the border. Defaults to `2px`.
 * @cssprop --umb-entity-frame-color - Accent colour for the border and tab background. Defaults to `--uui-color-focus`.
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

	static override styles = [
		UmbTextStyles,
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
				inset: 0;
				border: var(--umb-entity-frame-border-width, 2px) solid
					var(--umb-entity-frame-color, var(--uui-color-focus));
				border-radius: var(--uui-border-radius);
				border-top-right-radius: 0;
				box-sizing: border-box;
				pointer-events: none;
			}

			.tab {
				position: absolute;
				bottom: 100%;
				right: 0;
				background: var(--umb-entity-frame-color, var(--uui-color-focus));
				color: var(--uui-color-surface, white);
				padding: var(--uui-size-2) var(--uui-size-2) var(--uui-size-1);
				border-radius: var(--uui-border-radius) var(--uui-border-radius) 0 0;
				font-size: var(--uui-type-small-size);
				line-height: 1;
				pointer-events: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-frame': UmbEntityFrameElement;
	}
}
