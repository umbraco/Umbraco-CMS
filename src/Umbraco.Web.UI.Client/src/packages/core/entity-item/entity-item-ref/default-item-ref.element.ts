import type { UmbDefaultItemModel } from '../types.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-item-ref')
export class UmbDefaultItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDefaultItemModel;

	@property({ type: Boolean })
	standalone = false;

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node name=${this.item.name} ?standalone=${this.standalone} readonly>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbDefaultItemModel) {
		const icon = item.icon || 'icon-shape-triangle';
		return html`<umb-icon slot="icon" name=${icon}></umb-icon>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-item-ref': UmbDefaultItemRefElement;
	}
}
