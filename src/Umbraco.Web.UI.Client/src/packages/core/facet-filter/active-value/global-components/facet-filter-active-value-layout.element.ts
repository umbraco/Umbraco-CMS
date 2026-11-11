import { UmbRemoveEvent } from '@umbraco-cms/backoffice/event';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Layout for a facet filter active value: the filter label, the value and a remove button.
 * @element umb-facet-filter-active-value-layout
 * @slot - The value.
 * @fires remove - When the remove button is clicked.
 */
@customElement('umb-facet-filter-active-value-layout')
export class UmbFacetFilterActiveValueLayoutElement extends UmbLitElement {
	/**
	 * The label of the facet filter the value belongs to. Can be a localization key prefixed with `#`.
	 */
	@property({ type: String })
	label?: string;

	#onRemove = () => {
		this.dispatchEvent(new UmbRemoveEvent());
	};

	override render() {
		return html`
			${this.label ? html`<span id="label">${this.localize.string(this.label)}:</span>` : nothing}
			<slot></slot>
			<uui-button label=${this.localize.term('general_remove')} compact @click=${this.#onRemove}>
				<umb-icon name="icon-delete"></umb-icon>
			</uui-button>
		`;
	}

	static override styles = [
		css`
			:host {
				display: inline-flex;
				align-items: center;
				gap: var(--uui-size-space-1);
				padding: 0 0 0 var(--uui-size-space-3);
				font-size: 12px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-facet-filter-active-value-layout': UmbFacetFilterActiveValueLayoutElement;
	}
}
