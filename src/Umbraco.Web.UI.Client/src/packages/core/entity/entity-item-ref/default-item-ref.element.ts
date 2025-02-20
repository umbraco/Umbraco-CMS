import type { UmbDefaultItemModel } from '../types.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-item-ref')
export class UmbDefaultItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDefaultItemModel;

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node name=${this.item.name} ?readonly=${this.readonly} ?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbDefaultItemModel) {
		if (!item.icon) return;
		return html`<umb-icon slot="icon" name=${item.icon}></umb-icon>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-item-ref': UmbDefaultItemRefElement;
	}
}
