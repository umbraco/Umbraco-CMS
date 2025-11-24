import type { UmbItemModel } from '../types.js';
import { getItemFallbackIcon, getItemFallbackName } from '../utils.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-item-ref')
export class UmbDefaultItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbItemModel;

	@property({ type: Boolean })
	standalone = false;

	@property({ type: Boolean })
	selectable = false;

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.item.name ?? `${getItemFallbackName(this.item)}`}
				?standalone=${this.standalone}
				?selectable=${this.selectable}
				readonly>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbItemModel) {
		const icon = item.icon || getItemFallbackIcon();
		return html`<umb-icon slot="icon" name=${icon}></umb-icon>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-item-ref': UmbDefaultItemRefElement;
	}
}
