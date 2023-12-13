import { UmbBlockTypeBase } from '../../types.js';
import { UmbBlockTypeInputContext } from './input-block-type.context.js';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-input-block-type')
export class UmbInputBlockTypeElement<BlockType extends UmbBlockTypeBase = UmbBlockTypeBase> extends UmbLitElement {
	@property({ type: Array, attribute: false })
	public get value() {
		return this._items;
	}
	public set value(items) {
		this._items = items;
	}

	@state()
	private _items: Array<BlockType> = [];

	#context = new UmbBlockTypeInputContext<BlockType>(this);

	constructor() {
		super();

		this.observe(this.#context.types, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html` ${this._items?.map((item) => this.#renderItem(item))} ${this.#renderButton()} `;
	}

	#renderButton() {
		if (this._items.length > 0) return;
		return html`
			<uui-button id="add-button" look="placeholder" @click=${() => this.#context.create()} label="open">
				<uui-icon name="icon-add"></uui-icon>
				Add
			</uui-button>
		`;
	}

	#renderItem(item: BlockType) {
		return html`
			<uui-card-block-type>
				<uui-action-bar slot="actions">
					<uui-button label="Copy media">
						<uui-icon name="icon-documents"></uui-icon>
					</uui-button>
					<uui-button @click=${() => this.#context.requestRemoveItem(item.contentElementTypeKey)} label="Remove block">
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-block-type>
		`;
	}

	static styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
			}

			#add-button {
				text-align: center;
				height: 160px;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}
		`,
	];
}

export default UmbInputBlockTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-block-type': UmbInputBlockTypeElement;
	}
}
