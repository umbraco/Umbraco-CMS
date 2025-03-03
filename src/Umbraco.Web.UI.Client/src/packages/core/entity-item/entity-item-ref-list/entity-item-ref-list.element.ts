import type { UmbEntityItemModel } from '../types.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, customElement, html, nothing, property, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';

@customElement('umb-entity-item-ref-list')
export class UmbEntityItemRefListElement extends UmbLitElement {
	@property({ type: Array, attribute: false })
	private _items?: Array<UmbEntityItemModel> | undefined;
	public get items(): Array<UmbEntityItemModel> | undefined {
		return this._items;
	}
	public set items(value: Array<UmbEntityItemModel> | undefined) {
		this._items = value;
		this.#sorter.setModel(this._items);
	}

	@property({ type: Boolean })
	standalone = false;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public get readonly() {
		return this.#readonly;
	}
	public set readonly(value) {
		this.#readonly = value;

		if (this.#readonly) {
			this.#sorter.disable();
		} else {
			this.#sorter.enable();
		}
	}
	#readonly = false;

	/**
	 * Sets the input to sortable mode, meaning items can be sorted by dragging and dropping.
	 * @type {boolean}
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public get sortable() {
		return this.#sortable;
	}
	public set sortable(value) {
		this.#sortable = value;

		if (this.#sortable) {
			this.#sorter.enable();
		} else {
			this.#sorter.disable();
		}
	}
	#sortable = false;

	#sorter = new UmbSorterController<UmbEntityItemModel>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry.unique;
		},
		identifier: 'Umb.SorterIdentifier.UmbEntityItemRefListElement',
		itemSelector: 'umb-entity-item-ref',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.items = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	override render() {
		if (!this.items) return nothing;

		return html`
			<uui-ref-list>
				${repeat(
					this.items,
					(item) => item.unique,
					(item) =>
						html`<umb-entity-item-ref
							id=${item.unique}
							.item=${item}
							?readonly=${this.readonly}
							?standalone=${this.standalone}>
							${when(!this.readonly, () => html` <slot name="actions"></slot> `)}
						</umb-entity-item-ref>`,
				)}
			</uui-ref-list>
		`;
	}

	static override styles = [
		css`
			umb-entity-item-ref[drag-placeholder] {
				opacity: 0.2;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-item-ref-list': UmbEntityItemRefListElement;
	}
}
