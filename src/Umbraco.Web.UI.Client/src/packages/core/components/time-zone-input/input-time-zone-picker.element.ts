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
import type { TimeZone } from '@umbraco-cms/backoffice/utils';

/**
 * @element umb-input-time-zone-picker
 */
@customElement('umb-input-time-zone-picker')
export class UmbInputTimeZonePickerElement extends UUIFormControlMixin(UmbLitElement, '') {
	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: Array, reflect: true })
	public set options(value) {
		this.#options = value;
		this._filteredOptions = value;
	}
	public get options() {
		return this.#options;
	}
	#options: Array<TimeZone> = [];

	@state()
	private _filteredOptions: Array<TimeZone> = [];

	@query('#input')
	protected _input?: UUIComboboxElement;

	/**
	 *
	 */
	constructor() {
		super();
		this._filteredOptions = this.options;
	}

	async #onAdd() {
		const input = this._input;
		if (typeof input?.value !== 'string') return;
		this.dispatchEvent(new UmbTimeZoneAddEvent({ value: input.value }));
		input.value = '';
	}

	#onSearch(event: UUIComboboxEvent) {
		const searchTerm = (event.currentTarget as UUIComboboxElement)?.search;
		this._filteredOptions = this.options.filter(
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
				--uui-input-height: var(--uui-size-12);
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
