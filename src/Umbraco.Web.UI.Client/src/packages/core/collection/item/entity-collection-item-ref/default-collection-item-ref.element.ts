import type { UmbCollectionItemModel } from '../types.js';
import { getItemFallbackName, getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-collection-item-ref')
export class UmbDefaultCollectionItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbCollectionItemModel;

	@property({ type: Boolean })
	selectable = false;

	@property({ type: Boolean })
	selected = false;

	@property({ type: Boolean })
	selectOnly = false;

	@property({ type: Boolean })
	disabled = false;

	@property({ type: String })
	href?: string;

	#onSelected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbSelectedEvent(this.item.unique));
	}

	#onDeselected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbDeselectedEvent(this.item.unique));
	}

	override render() {
		if (!this.item) return nothing;

		return html`<div>MY COLLECTION ITEM REF ELEMENT</div>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-item-ref': UmbDefaultCollectionItemRefElement;
	}
}
