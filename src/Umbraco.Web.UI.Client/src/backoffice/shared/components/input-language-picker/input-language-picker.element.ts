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
	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	min?: number;

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
	 * @default undefined
	 */
	@property({ type: Number })
	max?: number;

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

	private _selectedIsoCodes: Array<string> = [];
	public get selectedIsoCodes(): Array<string> {
		return this._selectedIsoCodes;
	}
	public set selectedIsoCodes(isoCodes: Array<string>) {
		this._selectedIsoCodes = isoCodes;
		super.value = isoCodes.join(',');
	}

	@property()
	public set value(isoCodesString: string) {
		if (isoCodesString !== this._value) {
			this.selectedIsoCodes = isoCodesString.split(/[ ,]+/);
		}
	}

	@state()
	private _items: Array<LanguageResponseModel> = [];

	#pickerContext = new UmbLanguagePickerContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._selectedIsoCodes.length < this.min
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._selectedIsoCodes.length > this.max
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.selectedIsoCodes = selection));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	private _openPicker() {
		// TODO: add filter
		// filter: this.filter,
		this.#pickerContext.openPicker();
	}

	render() {
		return html`
			${this._items.map((item) => this._renderItem(item))}
			<uui-button
				id="add-button"
				look="placeholder"
				@click=${this._openPicker}
				label="open"
				?disabled="${this._selectedIsoCodes.length === this.max}"
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
}

export default UmbInputLanguagePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-language-picker': UmbInputLanguagePickerElement;
	}
}
