import type { UmbStaticFileItemModel } from '../../repository/item/types.js';
import { UmbStaticFilePickerInputContext } from './input-static-file.context.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-input-static-file')
export class UmbInputStaticFileElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#serializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * This is a minimum amount of selected files in this input.
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
	minMessage = 'This field need more files';

	/**
	 * This is a maximum amount of selected files in this input.
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
	maxMessage = 'This field exceeds the allowed amount of files';

	public set selection(paths: Array<string>) {
		this.#pickerContext.setSelection(paths);
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

	@property()
	public pickableFilter?: (item: UmbStaticFileItemModel) => boolean;

	@state()
	private _items?: Array<UmbStaticFileItemModel>;

	#pickerContext = new UmbStaticFilePickerInputContext(this);

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
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		if (!this._items) return nothing;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this._renderItem(item),
				)}
			</uui-ref-list>
			${this.#renderAddButton()}
		`;
	}

	#openPicker() {
		this.#pickerContext.openPicker({
			pickableFilter: this.pickableFilter,
			multiple: this.max === 1 ? false : true,
			hideTreeRoot: true,
		});
	}

	#renderAddButton() {
		if (this.max === 1 && this.selection.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label=${this.localize.term('general_choose')}></uui-button>
		`;
	}

	private _renderItem(item: UmbStaticFileItemModel) {
		if (!item.unique) return;
		return html`
			<uui-ref-node name=${item.name} .detail=${this.#serializer.toServerPath(item.unique) || ''}>
				<!-- TODO: implement is trashed, if we cant retrieve the item on the server (but only ask the server if we need to anyway...). <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> -->
				<uui-action-bar slot="actions">
					<uui-button
						label=${this.localize.term('general_remove')}
						@click=${() => this.#pickerContext.requestRemoveItem(item.unique)}></uui-button>
				</uui-action-bar>
			</uui-ref-node>
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

export default UmbInputStaticFileElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-static-file': UmbInputStaticFileElement;
	}
}
