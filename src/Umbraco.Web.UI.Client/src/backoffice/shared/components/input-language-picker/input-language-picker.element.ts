import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLanguagePickerContext } from './input-language-picker.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-input-language-picker')
export class UmbInputLanguagePickerElement extends FormControlMixin(UmbLitElement) {
	/**
	 * This is a minimum amount of selected items in this input.
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
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
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
	maxMessage = 'This field exceeds the allowed amount of items';

	@property({ type: Object, attribute: false })
	public filter: (language: LanguageResponseModel) => boolean = () => true;

	public get selectedIsoCodes(): Array<string> {
		return this.#pickerContext.getSelection();
	}
	public set selectedIsoCodes(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
	}

	@property()
	public set value(isoCodesString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIsoCodes = isoCodesString.split(/[ ,]+/);
	}

	@state()
	private _items: Array<LanguageResponseModel> = [];

	#pickerContext = new UmbLanguagePickerContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerContext.getSelection().length < this.min
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerContext.getSelection().length > this.max
		);

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	private _openPicker() {
		this.#pickerContext.openPicker({
			filter: this.filter,
		});
	}

	render() {
		return html`
			<uui-ref-list> ${this._items.map((item) => this._renderItem(item))} </uui-ref-list>
			<uui-button
				id="add-button"
				look="placeholder"
				@click=${this._openPicker}
				label="open"
				?disabled="${this._items.length === this.max}"
				>Add</uui-button
			>
		`;
	}

	private _renderItem(item: LanguageResponseModel) {
		if (!item.isoCode) return;
		return html`
			<!-- TODO: add language ref element -->
			<uui-ref-node name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.isoCode)}>
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#pickerContext.requestRemoveItem(item.isoCode!)} label="Remove ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputLanguagePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-language-picker': UmbInputLanguagePickerElement;
	}
}
