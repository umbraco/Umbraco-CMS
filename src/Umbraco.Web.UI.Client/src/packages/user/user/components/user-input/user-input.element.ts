import type { UmbUserItemModel } from '../../repository/index.js';
import { UmbUserPickerInputContext } from './user-input.context.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';

// TODO: Shall we rename to 'umb-input-user'? [LK]
@customElement('umb-user-input')
export class UmbUserInputElement extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(
	UmbLitElement,
	undefined,
) {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputUser',
		itemSelector: 'umb-entity-item-ref',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.selection = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Sets the input to required, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	required?: boolean;

	@property({ type: String })
	requiredMessage?: string;

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public get min(): number {
		return this.#pickerContext.min;
	}
	public set min(value: number) {
		this.#pickerContext.min = value;
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
	public get max(): number {
		return this.#pickerContext.max;
	}
	public set max(value: number) {
		this.#pickerContext.max = value;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'max-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property({ type: Array })
	public set selection(uniques: Array<string>) {
		this.#pickerContext.setSelection(uniques);
		this.#sorter.setModel(uniques);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property()
	public override set value(uniques: string | undefined) {
		this.selection = splitStringToArray(uniques);
	}
	public override get value(): string | undefined {
		return this.selection.join(',');
	}

	@state()
	private _items?: Array<UmbUserItemModel>;

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	#pickerContext = new UmbUserPickerInputContext(this);

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => !this.readonly && !!this.required && (this.value === undefined || this.value === null || this.value === ''),
		);

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerContext.getSelection().length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), '_observeSelection');
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), '_observerItems');
		this.observe(this.#pickerContext.statuses, (statuses) => (this._statuses = statuses), '_observeStatuses');
	}

	protected override getFormElement() {
		return undefined;
	}

	#openPicker() {
		this.#pickerContext.openPicker({});
	}

	#removeItem(unique: string) {
		this.#pickerContext.requestRemoveItem(unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selection.length >= this.max) return nothing;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				label=${this.localize.term('general_choose')}
				@click=${this.#openPicker}
				?disabled=${this.readonly}></uui-button>
		`;
	}

	#renderItems() {
		if (!this._statuses) return nothing;
		return html`
			<uui-ref-list>
				${repeat(
					this._statuses,
					(status) => status.unique,
					(status) => this.#renderItem(status),
				)}
			</uui-ref-list>
		`;
	}

	#renderItem(status: UmbRepositoryItemsStatus) {
		const unique = status.unique;
		const item = this._items?.find((x) => x.unique === unique);
		const isError = status.state.type === 'error';
		return html`
			<umb-entity-item-ref
				id=${unique}
				.item=${item}
				?error=${isError}
				.errorMessage=${status.state.error}
				.errorDetail=${isError ? unique : undefined}
				?standalone=${this.max === 1}
				?readonly=${this.readonly}>
				<uui-action-bar slot="actions">
					<uui-button
						label=${this.localize.term('general_remove')}
						@click=${() => this.#removeItem(unique)}></uui-button>
				</uui-action-bar>
			</umb-entity-item-ref>
		`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export default UmbUserInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-input': UmbUserInputElement;
	}
}
