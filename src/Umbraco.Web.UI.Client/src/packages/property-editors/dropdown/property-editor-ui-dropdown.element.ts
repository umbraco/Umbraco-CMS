import { ensureArray, updateItemsSelectedState } from '../utils/property-editor-ui-state-manager.js';
import { css, customElement, html, map, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbInputDropdownListElement } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-dropdown
 */
@customElement('umb-property-editor-ui-dropdown')
export class UmbPropertyEditorUIDropdownElement
	extends UmbFormControlMixin<Array<string> | string | undefined, typeof UmbLitElement, undefined>(
		UmbLitElement,
		undefined,
	)
	implements UmbPropertyEditorUiElement
{
	#selection: Array<string> = [];

	@state()
	private _multiple: boolean = false;

	@state()
	private _options: Array<Option & { invalid?: boolean }> = [];

	@property({ type: Array })
	public override set value(value: Array<string> | string | undefined) {
		this.#selection = ensureArray(value);
		// Update the selected state of existing options when value changes
		this.#updateSelectedState();
	}
	public override get value(): Array<string> | undefined {
		return this.#selection;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Sets the input to mandatory, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@property({ type: String })
	name?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const items = config.getValueByAlias('items');

		if (Array.isArray(items) && items.length > 0) {
			this._options =
				typeof items[0] === 'string'
					? items.map((item) => ({ name: item, value: item, selected: this.#selection.includes(item) }))
					: items.map((item) => ({
							name: item.name,
							value: item.value,
							selected: this.#selection.includes(item.value),
						}));

			// If selection includes a value that is not in the list, add it to the list
			this.#selection.forEach((value) => {
				if (!this._options.find((item) => item.value === value)) {
					this._options.push({
						name: `${value} (${this.localize.term('validation_legacyOption')})`,
						value,
						selected: true,
						invalid: true,
					});
				}
			});
		}

		this._multiple = config.getValueByAlias<boolean>('multiple') ?? false;
	}

	protected override firstUpdated() {
		if (this._multiple) {
			this.addFormControlElement(this.shadowRoot!.querySelector('select')!);
		} else {
			this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-dropdown-list')!);
		}

		if (!this.mandatory && !this._multiple) {
			this._options.unshift({ name: '', value: '', selected: false, invalid: false });
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputDropdownListElement }) {
		const value = event.target.value as string;
		this.#setValue(value ? [value] : []);
	}

	#onChangeMulitple(event: Event & { target: HTMLSelectElement }) {
		const selected = event.target.selectedOptions;
		const value = selected ? Array.from(selected).map((option) => option.value) : [];
		this.#setValue(value);
	}

	#setValue(value: Array<string> | string | null | undefined) {
		if (!value) return;
		const selection = ensureArray(value);
		this._options.forEach((item) => (item.selected = selection.includes(item.value)));
		this.value = value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	/**
	 * Updates the selected state of all options based on current selection.
	 * This fixes the issue where UI doesn't update when values are set programmatically.
	 */
	#updateSelectedState() {
		// Only update if we have options loaded
		if (this._options.length > 0) {
			// Update state only if changes are needed
			const updatedOptions = updateItemsSelectedState(this._options, this.#selection, 'selected');
			if (updatedOptions !== this._options) {
				this._options = updatedOptions;
			}
		}
	}

	override render() {
		return html`
			${when(
				this._multiple,
				() => this.#renderDropdownMultiple(),
				() => this.#renderDropdownSingle(),
			)}
			${this.#renderDropdownValidation()}
		`;
	}

	#renderDropdownMultiple() {
		if (this.readonly) {
			return html`<div>${this.value?.join(', ')}</div>`;
		}

		return html`
			<select id="native" multiple ?required=${this.mandatory} @change=${this.#onChangeMulitple}>
				${map(
					this._options,
					(item) => html`<option value=${item.value} ?selected=${item.selected}>${item.name}</option>`,
				)}
			</select>
		`;
	}

	#renderDropdownSingle() {
		return html`
			<umb-input-dropdown-list
				.name=${this.name}
				.options=${this._options}
				.required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-dropdown-list>
		`;
	}

	#renderDropdownValidation() {
		const selectionHasInvalids = this.#selection.some((value) => {
			const option = this._options.find((item) => item.value === value);
			return option ? option.invalid : false;
		});

		if (selectionHasInvalids) {
			return html`<div class="error"><umb-localize key="validation_legacyOptionDescription"></umb-localize></div>`;
		}

		return nothing;
	}

	static override readonly styles = [
		UUISelectElement.styles,
		css`
			#native {
				height: auto;
			}

			.error {
				color: var(--uui-color-invalid);
				font-size: var(--uui-font-size-small);
			}
		`,
	];
}

export default UmbPropertyEditorUIDropdownElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dropdown': UmbPropertyEditorUIDropdownElement;
	}
}
