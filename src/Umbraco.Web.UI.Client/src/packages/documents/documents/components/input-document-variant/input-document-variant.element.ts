import type { UmbDocumentItemModel } from '../../item/types.js';
import { UmbDocumentPickerInputVariantContext } from './input-document-variant.context.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbInteractionMemoriesChangeEvent } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';

@customElement('umb-input-document-variant')
export class UmbInputDocumentVariantElement extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(
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
	minMessage = 'This field needs more items';

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
	@property({ type: Boolean })
	required = false;
	@property({ type: String })
	requiredMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

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

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	@property({ type: Boolean, attribute: 'variant-aware' })
	public get variantAware(): boolean {
		return this.#variantAware;
	}
	public set variantAware(value: boolean) {
		this.#variantAware = value;
	}
	#variantAware = false;

	#pickerInputContext = new UmbDocumentPickerInputVariantContext(this);

	@property({ type: String })
	public get culture(): string | undefined {
		return this.#pickerInputContext?.culture;
	}
	public set culture(value: string | undefined) {
		this.#pickerInputContext.setCulture(value);
	}

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage,
			() => !this.readonly && this.required && this.selection.length === 0,
		);

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !this.readonly && !!this.min && this.selection.length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !this.readonly && !!this.max && this.selection.length > this.max,
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

		this.observe(this.#pickerInputContext.statuses, (statuses) => (this._statuses = statuses), '_observerStatuses');

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

	#onRemove(unique: string) {
		this.#pickerInputContext.requestRemoveItem(unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.selection.length > 0 && this.max === 1) return nothing;
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
		if (!this._statuses) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._statuses,
					(status) => status.unique,
					(status) => {
						const unique = status.unique;
						const item = this._items?.find((x) => x.unique === unique);
						const isError = status.state.type === 'error';
						return html`
							<umb-entity-item-ref
								id=${unique}
								.item=${item}
								.culture=${this.culture}
								?error=${isError}
								.errorMessage=${status.state.error}
								.errorDetail=${isError ? unique : undefined}
								?readonly=${this.readonly}
								?standalone=${this.max === 1}>
								${when(
									!this.readonly,
									() => html`
										<uui-action-bar slot="actions">
											<uui-button
												label=${this.localize.term('general_remove')}
												@click=${() => this.#onRemove(unique)}></uui-button>
										</uui-action-bar>
									`,
								)}
							</umb-entity-item-ref>
						`;
					},
				)}
			</uui-ref-list>
		`;
	}

	static override styles = [
		css`
			uui-ref-list:not(:has(:nth-child(1))) {
				margin-top: -20px;
				padding-top: 20px;
			}

			#btn-add {
				display: block;
			}
		`,
	];
}

export { UmbInputDocumentVariantElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-variant': UmbInputDocumentVariantElement;
	}
}
