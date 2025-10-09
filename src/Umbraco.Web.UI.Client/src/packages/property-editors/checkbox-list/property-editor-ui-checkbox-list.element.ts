import { ensureArray, updateItemsSelectedState } from '../utils/property-editor-ui-state-manager.js';
import './components/input-checkbox-list/input-checkbox-list.element.js';
import type {
	UmbCheckboxListItem,
	UmbInputCheckboxListElement,
} from './components/input-checkbox-list/input-checkbox-list.element.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-checkbox-list
 */
@customElement('umb-property-editor-ui-checkbox-list')
export class UmbPropertyEditorUICheckboxListElement
	extends UmbFormControlMixin<Array<string> | string | undefined, typeof UmbLitElement, undefined>(
		UmbLitElement,
		undefined,
	)
	implements UmbPropertyEditorUiElement
{
	#selection: Array<string> = [];

	@property({ type: Array })
	public override set value(value: Array<string> | string | undefined) {
		this.#selection = ensureArray(value);
		// Update the checked state of existing list items when value changes
		this.#updateCheckedState();
	}

	public override get value(): Array<string> | undefined {
		return this.#selection;
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const items = config.getValueByAlias('items');

		if (Array.isArray(items) && items.length) {
			this._list =
				typeof items[0] === 'string'
					? items.map((item) => ({ label: item, value: item, checked: this.#selection.includes(item) }))
					: items.map((item) => ({
							label: item.name,
							value: item.value,
							checked: this.#selection.includes(item.value),
						}));

			// If selection includes a value that is not in the list, add it to the list
			this.#selection.forEach((value) => {
				if (!this._list.find((item) => item.value === value)) {
					this._list.push({ label: value, value, checked: true, invalid: true });
				}
			});
		}
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

	@state()
	private _list: Array<UmbCheckboxListItem> = [];

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-checkbox-list')!);
	}

	#onChange(event: CustomEvent & { target: UmbInputCheckboxListElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	/**
	 * Updates the checked state of all list items based on current selection.
	 * This fixes the issue where UI doesn't update when values are set programmatically.
	 */
	#updateCheckedState() {
		// Only update if we have list items loaded
		if (this._list.length > 0) {
			// Update state only if changes are needed
			const updatedList = updateItemsSelectedState(this._list, this.#selection, 'checked');
			if (updatedList !== this._list) {
				this._list = updatedList;
			}
		}
	}

	override render() {
		return html`
			<umb-input-checkbox-list
				.list=${this._list}
				.required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				.selection=${this.#selection}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-checkbox-list>
		`;
	}
}

export default UmbPropertyEditorUICheckboxListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-checkbox-list': UmbPropertyEditorUICheckboxListElement;
	}
}
