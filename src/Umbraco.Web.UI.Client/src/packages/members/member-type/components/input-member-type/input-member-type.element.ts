import { UmbMemberTypePickerInputContext } from './input-member-type.context.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbRepositoryItemsStatus } from '@umbraco-cms/backoffice/repository';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';
import type { UmbUniqueItemModel } from '@umbraco-cms/backoffice/models';

import '@umbraco-cms/backoffice/entity-item';

@customElement('umb-input-member-type')
export class UmbInputMemberTypeElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
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

	public set selection(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
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
	private _items?: Array<UmbUniqueItemModel>;

	@state()
	private _statuses?: Array<UmbRepositoryItemsStatus>;

	#pickerContext = new UmbMemberTypePickerInputContext(this);

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

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
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

	override render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._statuses) return;
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

	#renderAddButton() {
		if (this.max === 1 && this.selection.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label="${this.localize.term('general_choose')}"
				>${this.localize.term('general_choose')}</uui-button
			>
		`;
	}

	#renderItem(status: UmbRepositoryItemsStatus) {
		const unique = status.unique;
		const item = this._items?.find((x) => x.unique === unique);
		const isError = status.state.type === 'error';

		// For error state, use umb-entity-item-ref
		if (isError) {
			return html`
				<umb-entity-item-ref
					id=${unique}
					?error=${true}
					.errorMessage=${status.state.error}
					.errorDetail=${unique}
					?standalone=${this.max === 1}>
					<uui-action-bar slot="actions">
						<uui-button
							label=${this.localize.term('general_remove')}
							@click=${() => this.#pickerContext.requestRemoveItem(unique)}></uui-button>
					</uui-action-bar>
				</umb-entity-item-ref>
			`;
		}

		// For successful items, use the member type specific component
		if (!item?.unique) return nothing;
		return html`
			<uui-ref-node-document-type name=${this.localize.string(item.name)}>
				${when(item.icon, () => html`<umb-icon slot="icon" name=${item.icon!}></umb-icon>`)}
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(unique)}
						label="Remove Member Type ${item.name}"
						>${this.localize.term('general_remove')}</uui-button
					>
				</uui-action-bar>
			</uui-ref-node-document-type>
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

export default UmbInputMemberTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-member-type': UmbInputMemberTypeElement;
	}
}
