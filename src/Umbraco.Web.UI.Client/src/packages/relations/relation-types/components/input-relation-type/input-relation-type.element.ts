import type { UmbRelationTypeItemModel } from '../../repository/item/types.js';
import { UMB_EDIT_RELATION_TYPE_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UmbRelationTypePickerInputContext } from './input-relation-type.context.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';

import '@umbraco-cms/backoffice/entity-item';

@customElement('umb-input-relation-type')
export class UmbInputRelationTypeElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
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
		identifier: 'Umb.SorterIdentifier.InputRelationType',
		itemSelector: 'uui-ref-node',
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
		this.#pickerContext.min = value;
	}
	public get min(): number {
		return this.#pickerContext.min;
	}

	/**
	 * Min validation message.
	 * @type {string}
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
		this.#pickerContext.max = value;
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message.
	 * @type {string}
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

	@property({ type: String })
	public override set value(uniques: string | undefined) {
		this.selection = splitStringToArray(uniques);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	@property({ type: Boolean, attribute: 'readonly' })
	readonly = false;

	@state()
	private _items?: Array<UmbRelationTypeItemModel>;

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	#pickerContext = new UmbRelationTypePickerInputContext(this);

	constructor() {
		super();

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

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), null);
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), null);
		this.observe(this.#pickerContext.statuses, (statuses) => (this._statuses = statuses), null);
	}

	protected override getFormElement() {
		return undefined;
	}

	#openPicker() {
		this.#pickerContext.openPicker();
	}

	#removeItem(unique: string) {
		this.#pickerContext.requestRemoveItem(unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.readonly || (this.max > 0 && this.selection.length >= this.max)) return nothing;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label="${this.localize.term('general_choose')}"></uui-button>
		`;
	}

	#renderItems() {
		if (!this._statuses) return nothing;
		return html`
			<uui-ref-list>
				${repeat(
					this._statuses,
					(status) => status.unique,
					(status) => {
						const unique = status.unique;
						const item = this._items?.find((x) => x.unique === unique);
						const isError = status.state.type === 'error';

						// For error state, use umb-entity-item-ref
						if (isError) {
							return html`
								<umb-entity-item-ref
									id=${unique}
									.item=${item}
									?error=${true}
									.errorMessage=${status.state.error}
									.errorDetail=${unique}
									?readonly=${this.readonly}
									?standalone=${this.max === 1}>
									${when(
										!this.readonly,
										() => html`
											<uui-action-bar slot="actions">
												<uui-button
													label=${this.localize.term('general_remove')}
													@click=${() => this.#removeItem(unique)}></uui-button>
											</uui-action-bar>
										`,
									)}
								</umb-entity-item-ref>
							`;
						}

						if (!item) return nothing;
						const href = UMB_EDIT_RELATION_TYPE_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique });
						return html`
							<uui-ref-node name=${item.name} id=${unique} href=${href} ?readonly=${this.readonly}>
								<umb-icon slot="icon" name="icon-trafic"></umb-icon>
								<uui-action-bar slot="actions">
									${when(
										!this.readonly,
										() => html`
											<uui-button
												label=${this.localize.term('general_remove')}
												@click=${() => this.#removeItem(unique)}></uui-button>
										`,
									)}
								</uui-action-bar>
							</uui-ref-node>
						`;
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

export default UmbInputRelationTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-relation-type': UmbInputRelationTypeElement;
	}
}
