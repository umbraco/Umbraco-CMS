import type { UmbDocumentItemModel } from '../../item/types.js';
import { UmbDocumentPickerInputContext } from './input-document.context.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbInteractionMemoriesChangeEvent } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

@customElement('umb-input-document')
export class UmbInputDocumentElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputDocument',
		itemSelector: 'umb-entity-item-ref',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.selection = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set min(value: number) {
		this.#pickerInputContext.min = value;
	}
	public get min(): number {
		return this.#pickerInputContext.min;
	}

	/**
	 * Min validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set max(value: number) {
		this.#pickerInputContext.max = value;
	}
	public get max(): number {
		return this.#pickerInputContext.max;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'max-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	public set selection(ids: Array<string>) {
		this.#pickerInputContext.setSelection(ids);
		this.#sorter.setModel(ids);
	}
	public get selection(): Array<string> {
		return this.#pickerInputContext.getSelection();
	}

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: Array })
	allowedContentTypeIds?: string[] | undefined;

	@property({ type: Boolean, attribute: 'include-trashed' })
	includeTrashed = false;

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
		super.value = selectionString; // Call the parent setter to ensure the value change is triggered in the FormControlMixin. [NL]
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
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

	@property({ type: Array, attribute: false })
	public get interactionMemories(): Array<UmbInteractionMemoryModel> | undefined {
		return this.#pickerInputContext.interactionMemory.getAllMemories();
	}
	public set interactionMemories(value: Array<UmbInteractionMemoryModel> | undefined) {
		this.#interactionMemories = value;
		value?.forEach((memory) => this.#pickerInputContext.interactionMemory.setMemory(memory));
	}

	#interactionMemories?: Array<UmbInteractionMemoryModel> = [];

	@state()
	private _items?: Array<UmbDocumentItemModel>;

	#pickerInputContext = new UmbDocumentPickerInputContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.selection.length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.selection.length > this.max,
		);

		this.observe(
			this.#pickerInputContext.selection,
			(selection) => (this.value = selection.join(',')),
			'_observeSelection',
		);

		this.observe(
			this.#pickerInputContext.selectedItems,
			(selectedItems) => (this._items = selectedItems),
			'_observerItems',
		);

		this.observe(
			this.#pickerInputContext.interactionMemory.memories,
			(memories) => {
				// only dispatch the event if the interaction memories have actually changed
				const isIdentical = jsonStringComparison(memories, this.#interactionMemories);

				if (!isIdentical) {
					this.#interactionMemories = memories;
					this.dispatchEvent(new UmbInteractionMemoriesChangeEvent());
				}
			},
			'_observeMemories',
		);
	}

	#openPicker() {
		this.#pickerInputContext.openPicker(
			{
				hideTreeRoot: true,
				startNode: this.startNode,
			},
			{
				allowedContentTypes: this.allowedContentTypeIds?.map((id) => ({
					unique: id,
					entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
				})),
				includeTrashed: this.includeTrashed,
			},
		);
	}

	#onRemove(item: UmbDocumentItemModel) {
		this.#pickerInputContext.requestRemoveItem(item.unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.selection.length >= this.max) return nothing;
		if (this.readonly && this.selection.length > 0) {
			return nothing;
		} else {
			return html`
				<uui-button
					id="btn-add"
					look="placeholder"
					@click=${this.#openPicker}
					label=${this.localize.term('general_choose')}
					?disabled=${this.readonly}></uui-button>
			`;
		}
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) =>
						html`<umb-entity-item-ref
							id=${item.unique}
							.item=${item}
							?readonly=${this.readonly}
							?standalone=${this.max === 1}>
							${when(
								!this.readonly,
								() => html`
									<uui-action-bar slot="actions">
										<uui-button
											label=${this.localize.term('general_remove')}
											@click=${() => this.#onRemove(item)}></uui-button>
									</uui-action-bar>
								`,
							)}
						</umb-entity-item-ref>`,
				)}
			</uui-ref-list>
		`;
	}

	static override styles = [
		css`
			#btn-add {
				display: block;
			}
		`,
	];
}

export { UmbInputDocumentElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document': UmbInputDocumentElement;
	}
}
