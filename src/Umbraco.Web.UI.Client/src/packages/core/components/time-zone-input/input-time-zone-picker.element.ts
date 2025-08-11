import {
	html,
	customElement,
	css,
	property,
	query,
	when,
	until,
	repeat,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { getTimeZoneList, type TimeZone } from '@umbraco-cms/backoffice/utils';
import { DateTime } from '@umbraco-cms/backoffice/external/luxon';

/**
 * @element umb-input-time-zone-picker
 */
@customElement('umb-input-time-zone-picker')
export class UmbInputTimeZonePickerElement extends UUIFormControlMixin(UmbLitElement, '') {
	private _options: Array<TimeZone> = [];
	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _filteredOptions: Array<TimeZone> = [];

	@query('#input')
	protected _input?: UUIComboboxElement;

	/**
	 *
	 */
	constructor() {
		super();
		this._options = this._filteredOptions = getTimeZoneList(undefined, DateTime.now());
	}

	async #onAdd() {
		const input = this._input;
		if (typeof input?.value !== 'string') return;
		this.dispatchEvent(new UmbTimeZoneAddEvent({ value: input.value }));
		input.value = '';
	}

	#onSearch(event: UUIComboboxEvent) {
		const searchTerm = (event.currentTarget as UUIComboboxElement)?.search;
		this._filteredOptions = this._options.filter(
			(option) => new RegExp(searchTerm, 'i').test(option.name) || option.offset === searchTerm,
		);
	}

	// Prevent valid events from bubbling outside the message element
	#onValid(event: Event) {
		event.stopPropagation();
	}

	// Prevent invalid events from bubbling outside the message element
	#onInvalid(event: Event) {
		event.stopPropagation();
	}

	public override async focus() {
		await this.updateComplete;
		this._input?.focus();
	}

	protected override getFormElement() {
		return undefined;
	}

	#renderTimeZoneOption = (option: Option) =>
		html`<uui-combobox-list-option .value=${option.value} .displayValue=${option.name}>
			${option.name}
		</uui-combobox-list-option>`;

	override render() {
		return html`
			<umb-form-validation-message id="validation-message" @invalid=${this.#onInvalid} @valid=${this.#onValid}>
				<uui-combobox
					id="input"
					pristine
					label="Value"
					@search="${this.#onSearch}"
					?disabled=${this.disabled}
					?readonly=${this.readonly}>
					<uui-combobox-list>
						${until(repeat(this._filteredOptions, this.#renderTimeZoneOption), html`Searching...`)}
					</uui-combobox-list>
				</uui-combobox>
			</umb-form-validation-message>

			${when(
				!this.readonly,
				() => html`
					<uui-button
						id="add-button"
						compact
						label="${this.localize.term('general_add')} ${this.value}"
						look="outline"
						color="positive"
						?disabled=${this.disabled || !this._input?.value}
						@click=${this.#onAdd}>
						<uui-icon name="icon-add"></uui-icon>
					</uui-button>
				`,
			)}
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				margin-bottom: var(--uui-size-space-3);
				gap: var(--uui-size-space-1);
			}

			#input {
				width: 100%;
				--uui-input-height: var(--uui-size-11);
			}

			#validation-message {
				flex: 1;
			}

			.handle {
				cursor: grab;
			}

			.handle:active {
				cursor: grabbing;
			}
		`,
	];
}

export class UmbTimeZoneAddEvent extends Event {
	#value: string;

	public constructor({ value }: { value: string }) {
		super('added', { bubbles: true, composed: true });
		this.#value = value;
	}

	public getValue() {
		return this.#value;
	}
}

export default UmbInputTimeZonePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-time-zone-picker': UmbInputTimeZonePickerElement;
	}
}
