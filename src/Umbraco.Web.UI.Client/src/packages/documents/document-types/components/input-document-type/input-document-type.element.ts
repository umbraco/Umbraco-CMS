import type { UmbDocumentTypeItemModel, UmbDocumentTypeTreeItemModel } from '../../types.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_MODAL } from '../../constants.js';
import { UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UmbDocumentTypePickerInputContext } from './input-document-type.context.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';

import '@umbraco-cms/backoffice/entity-item';

@customElement('umb-input-document-type')
export class UmbInputDocumentTypeElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
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
		identifier: 'Umb.SorterIdentifier.InputDocumentType',
		itemSelector: 'uui-ref-node-document-type',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.selection = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	/**
	 * Limits to only select Element Types
	 * @type {boolean}
	 * @default false
	 */
	@property({ attribute: false })
	elementTypesOnly: boolean = false;

	/**
	 * Limits to only select Document Types
	 * @type {boolean}
	 * @default false
	 */
	@property({ attribute: false })
	documentTypesOnly: boolean = false;

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
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
	 * @type {boolean}
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
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
	 * @type {boolean}
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
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
	readonly?: boolean;

	@state()
	private _items?: Array<UmbDocumentTypeItemModel>;

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	@state()
	private _editPath = '';

	#pickerContext = new UmbDocumentTypePickerInputContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_DOCUMENT_TYPE_WORKSPACE_MODAL)
			.addAdditionalPath('document-type')
			.onSetup(() => {
				return {};
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({});
			});

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

	#getPickableFilter() {
		if (this.documentTypesOnly) {
			/* TODO: We do not have the same model in the tree and during the search, so theoretically, we cannot use the same filter.
			The search item model does not include "isFolder," so it checks for falsy intentionally.
			We need to investigate getting this typed correctly. [MR] */
			return (x: UmbDocumentTypeTreeItemModel) => !x.isFolder && x.isElement === false;
		}
		if (this.elementTypesOnly) {
			return (x: UmbDocumentTypeTreeItemModel) => !x.isFolder && x.isElement;
		}
		return (x: UmbDocumentTypeTreeItemModel) => !x.isFolder;
	}

	#openPicker() {
		if (this.documentTypesOnly && this.elementTypesOnly) {
			throw new Error('You cannot set both documentTypesOnly and elementTypesOnly to true.');
		}

		const args: Parameters<UmbDocumentTypePickerInputContext['openPicker']>[1] = {};

		if (this.documentTypesOnly) {
			args.documentTypesOnly = true;
		} else if (this.elementTypesOnly) {
			args.elementTypesOnly = true;
		}

		this.#pickerContext.openPicker(
			{
				hideTreeRoot: true,
				pickableFilter: this.#getPickableFilter(),
			},
			args,
		);
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

						// For successful items, use the document type specific component
						if (!item) return nothing;
						const href = this._editPath + UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.generateLocal({ unique });
						return html`
							<uui-ref-node-document-type id=${unique} name=${this.localize.string(item.name)} href=${href}>
								${this.#renderIcon(item)}
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
							</uui-ref-node-document-type>
						`;
					},
				)}
			</uui-ref-list>
		`;
	}

	#renderIcon(item: UmbDocumentTypeItemModel) {
		if (!item.icon) return;
		return html`<umb-icon slot="icon" name=${item.icon}></umb-icon>`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export default UmbInputDocumentTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-type': UmbInputDocumentTypeElement;
	}
}
