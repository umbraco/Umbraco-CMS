import { UmbSorterController } from '../sorter.controller.js';
import type { UmbSorterConfig } from '../sorter.controller.js';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import { css, customElement, html, nothing, property, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIBlinkAnimationValue, UUIBlinkKeyframes } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-sortable-list
 * @fires UmbChangeEvent when the order of items has changed
 * @example
 * ```html
 * <umb-sortable-list
 *     .items=${this.items}
 *     .getUnique=${(item) => item.alias}
 *     .renderMethod=${(item) => html`
 *         <umb-sortable-list-item .unique=${item.alias}>
 *             <uui-input .value=${item.name}></uui-input>
 *             <uui-button slot="actions" label="Remove"></uui-button>
 *         </umb-sortable-list-item>
 *     `}
 *     @change=${this.#onSort}>
 * </umb-sortable-list>
 * ```
 */
@customElement('umb-sortable-list')
export class UmbSortableListElement<T = unknown> extends UmbLitElement {
	readonly #sorterConfig: UmbSorterConfig<T> = {
		getUniqueOfElement: (element) => element.dataset.sortEntryId,
		getUniqueOfModel: (model) => this.getUnique(model),
		itemSelector: 'umb-sortable-list-item',
		handleSelector: '.handle',
		onChange: ({ model }) => {
			this.items = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	};

	readonly #sorter = new UmbSorterController<T>(this, this.#sorterConfig);

	/**
	 * The selector used to find the items to sort within this element's shadow root.
	 * @type {string}
	 * @attr item-selector
	 */
	@property({ attribute: 'item-selector' })
	public get itemSelector(): string {
		return this.#sorterConfig.itemSelector;
	}
	public set itemSelector(value: string) {
		this.#sorterConfig.itemSelector = value || 'umb-sortable-list-item';
	}

	/**
	 * The selector used to find the drag handle within an item. If not found, the whole item is draggable.
	 * @type {string}
	 * @attr handle-selector
	 */
	@property({ attribute: 'handle-selector' })
	public get handleSelector(): string {
		return this.#sorterConfig.handleSelector ?? '';
	}
	public set handleSelector(value: string) {
		this.#sorterConfig.handleSelector = value || '.handle';
	}

	/**
	 * The items to render and sort.
	 * @type {Array<T>}
	 */
	@property({ type: Array })
	public get items(): Array<T> {
		return this.#items;
	}
	public set items(items: Array<T> | undefined) {
		this.#items = items ?? [];
		this.#sorter.setModel(this.#items);
	}
	#items: Array<T> = [];

	/**
	 * Resolves the unique identifier of an item, used to keep track of items while sorting.
	 * @type {(item: T) => string}
	 */
	@property({ attribute: false })
	public getUnique: (item: T) => string = (): string => '';

	/**
	 * Renders the markup for an item. The returned template must be rooted at an element carrying
	 * the item's unique identifier via a `data-sort-entry-id` attribute, e.g. `umb-sortable-list-item`'s `unique` property.
	 * @type {(item: T, index: number) => TemplateResult}
	 */
	@property({ attribute: false })
	public renderMethod: (item: T, index: number) => TemplateResult = (): TemplateResult => html``;

	/**
	 * Disables sorting.
	 * @type {boolean}
	 * @attr
	 */
	@property({ type: Boolean, reflect: true })
	public set disabled(value: boolean) {
		this.#disabled = value;
		this.#updateSorterState();
	}
	public get disabled(): boolean {
		return this.#disabled;
	}
	#disabled = false;

	#updateSorterState() {
		if (this.#disabled) {
			this.#sorter.disable();
		} else {
			this.#sorter.enable();
		}
	}

	override render() {
		if (!this.items.length) return nothing;
		return repeat(
			this.items,
			(item) => this.getUnique(item),
			(item, index) => this.renderMethod(item, index),
		);
	}

	static override readonly styles = [
		UUIBlinkKeyframes,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: 1px;
			}

			[drag-placeholder] {
				animation: ${UUIBlinkAnimationValue};
				background-color: color-mix(in srgb, var(--uui-color-interactive-emphasis) 15%, transparent) !important;
				border-radius: var(--uui-border-radius);
				border: 2px solid var(--uui-color-interactive-emphasis);
			}
		`,
	];
}

export default UmbSortableListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-sortable-list': UmbSortableListElement;
	}
}
