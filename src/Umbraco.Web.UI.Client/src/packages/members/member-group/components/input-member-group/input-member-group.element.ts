import type { UmbMemberGroupItemModel } from '../../types.js';
import { UmbMemberGroupPickerInputContext } from './input-member-group.context.js';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-input-member-group')
export class UmbInputMemberGroupElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputMemberGroup',
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
		this.#sorter.setModel(ids);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property({ type: Array })
	allowedContentTypeIds?: string[] | undefined;

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	@property({ type: Object, attribute: false })
	public filter: (memberGroup: UmbMemberGroupItemModel) => boolean = () => true;

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
	private _editMemberGroupPath = '';

	@state()
	private _items?: Array<UmbMemberGroupItemModel>;

	#pickerContext = new UmbMemberGroupPickerInputContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('member-group')
			.onSetup(() => {
				return { data: { entityType: 'member-group', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMemberGroupPath = routeBuilder({});
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
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), '_observeItems');
	}

	protected override getFormElement() {
		return undefined;
	}

	#openPicker() {
		this.#pickerContext.openPicker({
			filter: this.filter,
		});
	}

	#removeItem(item: UmbMemberGroupItemModel) {
		this.#pickerContext.requestRemoveItem(item.unique);
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-ref-list>
		`;
	}

	#renderAddButton() {
		if (this.max === 1 && this.selection.length >= this.max) return nothing;
		if (this.readonly && this.selection.length > 0) {
			return nothing;
		} else {
			return html`<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label=${this.localize.term('general_choose')}
				?disabled=${this.readonly}></uui-button>`;
		}
	}

	#renderItem(item: UmbMemberGroupItemModel) {
		if (!item.unique) return nothing;
		return html`
			<uui-ref-node
				name=${item.name}
				id=${item.unique}
				href="${this._editMemberGroupPath}edit/${item.unique}"
				?readonly=${this.readonly}>
				<uui-action-bar slot="actions"> ${this.#renderRemoveButton(item)} </uui-action-bar>
				<umb-icon slot="icon" name="icon-users"></umb-icon>
			</uui-ref-node>
		`;
	}

	#renderRemoveButton(item: UmbMemberGroupItemModel) {
		if (this.readonly) return nothing;
		return html`<uui-button
			@click=${() => this.#removeItem(item)}
			label=${this.localize.term('general_remove')}></uui-button>`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}

			uui-ref-node[drag-placeholder] {
				opacity: 0.2;
			}
		`,
	];
}

export default UmbInputMemberGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-member-group': UmbInputMemberGroupElement;
	}
}
