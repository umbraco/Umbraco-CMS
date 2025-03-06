import { css, customElement, html, map, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-dropdown
 */
@customElement('umb-property-editor-ui-dropdown')
export class UmbPropertyEditorUIDropdownElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#selection: Array<string> = [];

	@property({ type: Array })
	public set value(value: Array<string> | string | undefined) {
		this.#selection = Array.isArray(value) ? value : value ? [value] : [];
	}
	public get value(): Array<string> | undefined {
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

	@state()
	private _multiple: boolean = false;

	@state()
	private _options: Array<Option & { invalid?: boolean }> = [];

	#onChange(event: UUISelectEvent) {
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
		this.value = value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return this._multiple ? this.#renderDropdownMultiple() : this.#renderDropdownSingle();
	}

	#renderDropdownMultiple() {
		if (this.readonly) {
			return html`<div>${this.value?.join(', ')}</div>`;
		}

		return html`
			<select id="native" multiple @change=${this.#onChangeMulitple}>
				${map(
					this._options,
					(item) => html`<option value=${item.value} ?selected=${item.selected}>${item.name}</option>`,
				)}
			</select>
			${this.#renderDropdownValidation()}
		`;
	}

	#renderDropdownSingle() {
		return html`
			<umb-input-dropdown-list
				.options=${this._options}
				@change=${this.#onChange}
				?readonly=${this.readonly}></umb-input-dropdown-list>
			${this.#renderDropdownValidation()}
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
