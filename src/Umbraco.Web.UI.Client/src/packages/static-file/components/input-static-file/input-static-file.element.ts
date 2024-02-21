import type { UmbStaticFileItemModel } from '../../repository/item/types.js';
import { UmbStaticFilePickerContext } from './input-static-file.context.js';
import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-static-file')
export class UmbInputStaticFileElement extends FormControlMixin(UmbLitElement) {
	/**
	 * This is a minimum amount of selected files in this input.
	 * @type {number}
	 * @attr
	 * @default 0
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
	minMessage = 'This field need more files';

	/**
	 * This is a maximum amount of selected files in this input.
	 * @type {number}
	 * @attr
	 * @default Infinity
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
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of files';

	public get selectedPaths(): Array<string> {
		return this.#pickerContext.getSelection();
	}
	public set selectedPaths(paths: Array<string>) {
		this.#pickerContext.setSelection(paths);
	}

	@property()
	// get value is handled by super class.
	public set value(pathsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedPaths = splitStringToArray(pathsString);
	}

	@state()
	private _items?: Array<UmbStaticFileItemModel>;

	#pickerContext = new UmbStaticFilePickerContext(this);

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

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`
			${this._items
				? html` <uui-ref-list
						>${repeat(
							this._items,
							(item) => item.unique,
							(item) => this._renderItem(item),
						)}
				  </uui-ref-list>`
				: ''}
			${this.#renderAddButton()}
		`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selectedPaths.length >= this.max) return;
		return html`<uui-button
			id="add-button"
			look="placeholder"
			@click=${() => this.#pickerContext.openPicker()}
			label=${this.localize.term('general_add')}></uui-button>`;
	}

	private _renderItem(item: UmbStaticFileItemModel) {
		if (!item.unique) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.unique)}>
				<!-- TODO: implement is trashed <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> -->
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.unique)}
						label="Remove file ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	static styles = [
		css`
			#add-button {
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
