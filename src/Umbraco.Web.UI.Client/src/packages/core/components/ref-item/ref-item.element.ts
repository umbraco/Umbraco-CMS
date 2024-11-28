import { customElement, css, property, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UUIRefEvent, UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';

const elementName = 'umb-ref-item';

@customElement(elementName)
export class UmbRefItemElement extends UmbElementMixin(UUIRefNodeElement) {
	@property({ type: String })
	icon = '';

	constructor() {
		super();

		this.selectable = true;

		this.addEventListener(UUIRefEvent.OPEN, () => this.dispatchEvent(new Event('click')));
	}

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);

		if (this.icon) {
			const umbIcon = document.createElement('umb-icon');
			umbIcon.setAttribute('name', this.icon);
			this.shadowRoot?.querySelector('#icon')?.replaceWith(umbIcon);
		}
	}

	static override styles = [
		...UUIRefNodeElement.styles,
		css`
			:host {
				padding: calc(var(--uui-size-4) + 1px);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbRefItemElement;
	}
}
