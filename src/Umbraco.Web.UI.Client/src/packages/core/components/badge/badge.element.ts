import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, property, css, LitElement, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-badge
 * @description A wrapper for the uui-badge component with position fixed support to go on top of other elements.
 * @augments {LitElement}
 */
@customElement('umb-badge')
export class UmbBadgeElement extends LitElement {
	/**
	 * Changes the look of the button to one of the predefined, symbolic looks.
	 * @type {"default" | "positive" | "warning" | "danger"}
	 * @attr
	 * @default "default"
	 */
	@property({ type: String })
	color?: UUIInterfaceColor;

	/**
	 * Changes the look of the button to one of the predefined, symbolic looks.
	 * @type {"default" | "primary" | "secondary" | "outline" | "placeholder"}
	 * @attr
	 * @default "default"
	 */
	@property({ type: String })
	look?: UUIInterfaceLook;

	/**
	 * Bring attention to this badge by applying a bounce animation.
	 * @type boolean
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean })
	attention?: boolean;

	override render() {
		return html`<uui-badge color=${ifDefined(this.color)} look=${ifDefined(this.look)} ?attention=${this.attention}
			><slot></slot
		></uui-badge>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				position: absolute;
				anchor-name: --umb-badge-anchor;
				/** because inset has no effect on uui-badge in this case, we then apply it here: */
				inset: var(--uui-badge-inset, -8px -8px auto auto);
			}

			@supports (position-anchor: --my-name) {
				uui-badge {
					position: fixed;
					position-anchor: --umb-badge-anchor;
					z-index: 1;
					top: anchor(top);
					right: anchor(right);
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-badge': UmbBadgeElement;
	}
}
