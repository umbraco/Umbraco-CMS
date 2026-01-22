import { UmbEntityDataPickerInputContext } from './input-entity-data.context.js';
import {
	css,
	html,
	customElement,
	property,
	state,
	repeat,
	nothing,
	when,
	ifDefined,
} from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray, type UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbCollectionLayoutConfiguration } from '@umbraco-cms/backoffice/collection';

@customElement('umb-input-entity-data')
export class UmbInputEntityDataElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputEntityData',
		itemSelector: 'umb-entity-item-ref',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.selection = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	public set dataSourceAlias(value: string | undefined) {
		this.#pickerInputContext.setDataSourceAlias(value);
	}
	public get dataSourceAlias(): string | undefined {
		return this.#pickerInputContext.getDataSourceAlias();
	}

	public set dataSourceConfig(config: UmbConfigCollectionModel | undefined) {
		this.#pickerInputContext.setDataSourceConfig(config);
	}
	public get dataSourceConfig(): UmbConfigCollectionModel | undefined {
		return this.#pickerInputContext.getDataSourceConfig();
	}

	public set pickerViews(value: Array<UmbCollectionLayoutConfiguration> | undefined) {
		this.#pickerInputContext.setPickerViews(value);
	}
	public get pickerViews(): Array<UmbCollectionLayoutConfiguration> | undefined {
		return this.#pickerInputContext.getPickerViews();
	}

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

	@property({ type: Array })
	public set selection(uniques: Array<string>) {
		this.#pickerInputContext.setSelection(uniques);
		this.#sorter.setModel(uniques);
	}
	public get selection(): Array<string> {
		return this.#pickerInputContext.getSelection();
	}

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

	@state()
	private _items: Array<UmbItemModel> = [];

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	@state()
	private _modalRoute?: string;

	#pickerInputContext = new UmbEntityDataPickerInputContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerInputContext.getSelection().length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerInputContext.getSelection().length > this.max,
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

		this.observe(this.#pickerInputContext.modalRoute, (modalRoute) => {
			this._modalRoute = modalRoute;
		});
	}

	protected override getFormElement() {
		return undefined;
	}

	#onRemove(unique: string) {
		this.#pickerInputContext.requestRemoveItem(unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selection.length >= this.max) return nothing;
		if (this.readonly && this.selection.length > 0) return nothing;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				href=${ifDefined(this._modalRoute)}
				label="${this.localize.term('general_choose')}"
				?disabled=${this.readonly}></uui-button>
		`;
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
						return html`<umb-entity-item-ref
							id=${unique}
							.item=${item}
							?error=${status.state.type === 'error'}
							.errorMessage=${status.state.error}
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
						</umb-entity-item-ref>`;
					},
				)}
			</uui-ref-list>
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-entity-data': UmbInputEntityDataElement;
	}
}
