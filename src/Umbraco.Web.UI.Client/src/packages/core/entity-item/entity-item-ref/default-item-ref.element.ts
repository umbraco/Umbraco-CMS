import type { UmbItemModel } from '../types.js';
import { getItemFallbackIcon, getItemFallbackName } from '../utils.js';
import { customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWithOptionalDescriptionModel } from '@umbraco-cms/backoffice/models';

type UmbDefaultItemRefItemModel = UmbItemModel & UmbWithOptionalDescriptionModel;

@customElement('umb-default-item-ref')
export class UmbDefaultItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDefaultItemRefItemModel;

	@property({ type: Boolean })
	standalone = false;

	@property({ type: Boolean })
	selectable = false;

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.item.name ?? `${getItemFallbackName(this.item)}`}
				detail=${ifDefined(this.item.description ?? undefined)}
				?standalone=${this.standalone}
				?selectable=${this.selectable}
				readonly>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbDefaultItemRefItemModel) {
		const icon = item.icon || getItemFallbackIcon();
		return html`<umb-icon slot="icon" name=${icon}></umb-icon>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-item-ref': UmbDefaultItemRefElement;
	}
}
