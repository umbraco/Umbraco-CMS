import { customElement, css, property, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-ref-item')
export class UmbRefItemElement extends UmbElementMixin(UUIRefNodeElement) {
	@property({ type: String })
	icon? = '';

	/**
	 * The maximum number of lines the detail text may span before it is truncated. Leave it unset to keep the
	 * detail on a single line.
	 * @type {number}
	 * @attr max-detail-lines
	 */
	@property({ type: Number, attribute: 'max-detail-lines', reflect: true })
	maxDetailLines?: number;

	#iconElement = document.createElement('umb-icon');

	protected override willUpdate(changedProperties: PropertyValues): void {
		super.willUpdate(changedProperties);

		if (changedProperties.has('maxDetailLines')) {
			// The detail lives in the shadow root, so the line count is handed to the styles as a custom property.
			if (this.maxDetailLines === undefined) {
				this.style.removeProperty('--umb-ref-item-max-detail-lines');
			} else {
				this.style.setProperty('--umb-ref-item-max-detail-lines', this.maxDetailLines.toString());
			}
		}
	}

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);

		if (this.icon) {
			// Temporary fix for the icon appending, this could in the future be changed to override a renderIcon method, or other ways to make this happen without appending children.
			this.#iconElement.setAttribute('slot', 'icon');
			this.#iconElement.setAttribute('name', this.icon);
			this.appendChild(this.#iconElement);
		}
	}

	static override styles = [
		...UUIRefNodeElement.styles,
		css`
			:host {
				padding-top: var(--uui-size-3);
				padding-bottom: var(--uui-size-3);
			}

			:host([max-detail-lines]) #detail {
				display: -webkit-box;
				-webkit-line-clamp: var(--umb-ref-item-max-detail-lines, 1);
				-webkit-box-orient: vertical;
				white-space: normal;
				overflow: hidden;
				text-overflow: ellipsis;
				overflow-wrap: anywhere;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-item': UmbRefItemElement;
	}
}
