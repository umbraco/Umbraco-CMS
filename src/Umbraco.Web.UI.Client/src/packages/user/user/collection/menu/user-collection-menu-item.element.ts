import type { UmbUserDetailModel } from '../../types.js';
import { html, customElement, css, state, property, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCollectionMenuItemContext } from '@umbraco-cms/backoffice/collection';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-collection-menu-item')
export class UmbUserCollectionMenuItemElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	set item(newVal: UmbUserDetailModel) {
		this._item = newVal;

		if (this._item) {
			this.#initItem();
		}
	}
	get item(): UmbUserDetailModel | undefined {
		return this._item;
	}
	protected _item?: UmbUserDetailModel;

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
				label=${item?.name}
				?selectable=${this._isSelectable}
				?selected=${this._isSelected}
				@selected=${() => this.#api?.select()}
				@deselected=${() => this.#api?.deselect()}>
				<umb-user-avatar
					slot="icon"
					.name=${item.name}
					.kind=${item.kind}
					.imgUrls=${item.avatarUrls}></umb-user-avatar>
			</uui-menu-item>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			umb-user-avatar {
				font-size: 10px;
			}
		`,
	];
}

export { UmbUserCollectionMenuItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection-menu-item': UmbUserCollectionMenuItemElement;
	}
}
