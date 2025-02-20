import { customElement, css, property, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-ref-item')
export class UmbRefItemElement extends UmbElementMixin(UUIRefNodeElement) {
	@property({ type: String })
	icon = '';

	#iconElement = document.createElement('umb-icon');

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);

		// Temporary fix for the icon appending, this could in the future be changed to override a renderIcon method, or other ways to make this happen without appending children.
		this.#iconElement.setAttribute('slot', 'icon');
		this.#iconElement.setAttribute('name', this.icon);
		this.appendChild(this.#iconElement);
	}

	static override styles = [
		...UUIRefNodeElement.styles,
		css`
			:host {
				padding-top: var(--uui-size-3);
				padding-bottom: var(--uui-size-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-item': UmbRefItemElement;
	}
}
