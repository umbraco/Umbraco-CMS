import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @description
 * This element is used to render a expose button that goes on top of a block.
 */
@customElement('umb-block-overlay-expose-button')
export class UmbBlockOverlayExposeButtonElement extends UmbLitElement {
	@property({ attribute: false })
	contentTypeName?: string;

	override render() {
		return this.contentTypeName
			? html`<uui-button look="secondary"
					><uui-icon name="icon-add"></uui-icon>
					<umb-localize key="blockEditor_createThisFor" .args=${[this.contentTypeName]}></umb-localize
				></uui-button>`
			: nothing;
	}

	static override styles = [
		css`
			:host {
				position: absolute;
				inset: 0;
			}

			uui-button {
				position: absolute;
				inset: 0;
				opacity: 0.8;
			}

			:host:hover uui-button {
				opacity: 1;
			}
		`,
	];
}

export default UmbBlockOverlayExposeButtonElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-overlay-expose-button': UmbBlockOverlayExposeButtonElement;
	}
}
