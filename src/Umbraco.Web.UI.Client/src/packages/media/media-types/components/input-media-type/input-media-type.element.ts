import type { UmbMediaTypeItemModel } from '../../types.js';
import { UmbMediaTypePickerInputContext } from './input-media-type.context.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';

import '@umbraco-cms/backoffice/entity-item';

@customElement('umb-input-media-type')
export class UmbInputMediaTypeElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputMediaType',
		itemSelector: 'uui-ref-node-document-type',
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
		this.#pickerContext.max = value;
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
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
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	@state()
	private _items?: Array<UmbMediaTypeItemModel>;

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	@state()
	private _editPath = '';

	#pickerContext = new UmbMediaTypePickerInputContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media-type')
			.onSetup(() => {
				return { data: { entityType: 'media-type', preset: {} } };
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
		return (x: UmbTreeItemModel) => !x.isFolder;
	}

	#openPicker() {
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
			pickableFilter: this.#getPickableFilter(),
		});
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
									?standalone=${this.max === 1}>
									<uui-action-bar slot="actions">
										<uui-button
											label=${this.localize.term('general_remove')}
											@click=${() => this.#removeItem(unique)}></uui-button>
									</uui-action-bar>
								</umb-entity-item-ref>
							`;
						}

						// For successful items, use the media type specific component
						if (!item) return nothing;
						const href = `${this._editPath}edit/${unique}`;
						return html`
							<uui-ref-node-document-type name=${this.localize.string(item.name)} id=${unique}>
								${this.#renderIcon(item)}
								<uui-action-bar slot="actions">
									<uui-button href=${href} label=${this.localize.term('general_open')}></uui-button>
									<uui-button
										label=${this.localize.term('general_remove')}
										@click=${() => this.#removeItem(unique)}></uui-button>
								</uui-action-bar>
							</uui-ref-node-document-type>
						`;
					},
				)}
			</uui-ref-list>
		`;
	}

	#renderIcon(item: UmbMediaTypeItemModel) {
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

export default UmbInputMediaTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-media-type': UmbInputMediaTypeElement;
	}
}
