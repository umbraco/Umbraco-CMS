import type { UmbCollectionItemModel } from '../../../item/types.js';
import type { UmbCollectionMenuItemContext } from '../collection-menu-item-context.interface.js';
import { html, state, property, customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { getItemFallbackIcon, getItemFallbackName } from '@umbraco-cms/backoffice/entity-item';

@customElement('umb-default-collection-menu-item')
export class UmbDefaultCollectionMenuItemElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	set item(newVal: UmbCollectionItemModel) {
		this._item = newVal;

		if (this._item) {
			this.#initItem();
		}
	}
	get item(): UmbCollectionItemModel | undefined {
		return this._item;
	}
	protected _item?: UmbCollectionItemModel;

	@property({ type: Object, attribute: false })
	public set api(value: UmbCollectionMenuItemContext | undefined) {
		this.#api = value;

		if (this.#api) {
			this.observe(this.#api.isSelectable, (value) => (this._isSelectable = value));
			this.observe(this.#api.isSelected, (value) => (this._isSelected = value));
			this.#initItem();
		}
	}
	public get api(): UmbCollectionMenuItemContext | undefined {
		return this.#api;
	}
	#api: UmbCollectionMenuItemContext | undefined;

	@state()
	protected _isActive = false;

	@state()
	protected _isSelected = false;

	@state()
	protected _isSelectable = false;

	#initItem() {
		if (!this.#api) return;
		if (!this._item) return;
		this.#api.setItem(this._item);
	}

	override render() {
		const item = this._item;
		if (!item) return nothing;

		return html`
			<uui-menu-item
				label=${item?.name ?? getItemFallbackName(item)}
				?selectable=${this._isSelectable}
				?selected=${this._isSelected}
				@selected=${() => this.#api?.select()}
				@deselected=${() => this.#api?.deselect()}>
				<umb-icon slot="icon" name=${item.icon ?? getItemFallbackIcon()}></umb-icon>
			</uui-menu-item>
		`;
	}

	static override styles = [UmbTextStyles];
}

export { UmbDefaultCollectionMenuItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-menu-item': UmbDefaultCollectionMenuItemElement;
	}
}
