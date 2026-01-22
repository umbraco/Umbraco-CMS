import type { UmbEntityCollectionItemElement } from '../entity-collection-item-element.interface.js';
import type { UmbCollectionItemModel } from '../types.js';
import { getItemFallbackName, getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWithOptionalThumbnailModel } from '@umbraco-cms/backoffice/models';

type UmbDefaultCollectionItemCardItemModel = UmbCollectionItemModel & UmbWithOptionalThumbnailModel;

@customElement('umb-default-collection-item-card')
export class UmbDefaultCollectionItemCardElement extends UmbLitElement implements UmbEntityCollectionItemElement {
	@property({ type: Object })
	item?: UmbDefaultCollectionItemCardItemModel;

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

		return html`
			<umb-card
				name=${this.item.name ?? `${getItemFallbackName(this.item)}`}
				href=${ifDefined(this.href)}
				?selectable=${this.selectable}
				?select-only=${this.selectOnly}
				?selected=${this.selected}
				?disabled=${this.disabled}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}
				?readonly=${!this.href}
				background-color="var(--uui-color-surface)">
				<slot name="actions" slot="actions"></slot>
				${this.item.thumbnail ? this.#renderThumbnail(this.item) : this.#renderIcon(this.item)}
			</umb-card>
		`;
	}

	#renderThumbnail(item: UmbDefaultCollectionItemCardItemModel) {
		if (!item.thumbnail) return nothing;
		return html`<img src=${item.thumbnail.src} alt=${item.thumbnail.alt || item.name || ''} />`;
	}

	#renderIcon(item: UmbDefaultCollectionItemCardItemModel) {
		const icon = item.icon || getItemFallbackIcon();
		return html`<umb-icon name=${icon}></umb-icon>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-item-card': UmbDefaultCollectionItemCardElement;
	}
}
