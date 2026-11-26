import type { UmbInputValueWithLabelElement } from './input-value-with-label.element.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { escapeHTML } from '@umbraco-cms/backoffice/utils';
import './input-value-with-label.element.js';

/**
 * An option as it is stored in the configuration of a value list: the value and the label.
 */
export interface UmbValueWithLabelConfiguration {
	value: string;
	label: string;
}

interface UmbValueWithLabelEntry {
	id: string;
	value: string;
	label: string;
	isNew?: boolean;
}

// Pressing a button must not blur the field being edited, or an error it then shows can move the button before the click lands.
const preventFocusLoss = (event: MouseEvent) => event.preventDefault();

/**
 * @element umb-input-values-with-labels
 */
@customElement('umb-input-values-with-labels')
export class UmbInputValuesWithLabelsElement extends UmbFormControlMixin<undefined | string, typeof UmbLitElement>(
	UmbLitElement,
) {
	readonly #sorter = new UmbSorterController<UmbValueWithLabelEntry>(this, {
		getUniqueOfElement: (element) => element.dataset.sortEntryId,
		getUniqueOfModel: (entry) => entry.id,
		identifier: 'Umb.SorterIdentifier.ValuesWithLabels',
		itemSelector: '.option',
		containerSelector: '#sorter-wrapper',
		onChange: ({ model }) => {
			this._entries = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	@state()
	private _entries: Array<UmbValueWithLabelEntry> = [];

	/**
	 * The options, each with a value and a label.
	 * @type {Array<UmbValueWithLabelConfiguration>}
	 */
	@property({ type: Array })
	public set items(items: Array<UmbValueWithLabelConfiguration>) {
		if (JSON.stringify(items) === JSON.stringify(this.items)) return;

		this._entries = items.map(({ value, label }) => ({
			id: crypto.randomUUID(),
			value,
			label: label ?? '',
		}));
		this.#sorter.setModel(this._entries);
	}
	public get items(): Array<UmbValueWithLabelConfiguration> {
		return this._entries.map(({ value, label }) => ({ value, label }));
	}

	/**
	 * Makes the input readonly
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public set readonly(value: boolean) {
		this.#readonly = value;
		if (value) this.#sorter.disable();
	}
	public get readonly() {
		return this.#readonly;
	}
	#readonly = false;

	constructor() {
		super();

		// The label is checked first: typing a repeated label into an added option also repeats the value filled in from it.
		this.#addUniquenessValidator('general_label', () => this.#duplicateLabel());
		this.#addUniquenessValidator('general_value', () => this.#duplicateValue());
	}

	#addUniquenessValidator(fieldKey: UmbKnownLocalizationKey, getDuplicate: () => string | undefined) {
		this.addValidator(
			'customError',
			() =>
				this.localize.term(
					'validation_duplicateFieldValue',
					this.localize.term(fieldKey),
					escapeHTML(getDuplicate() ?? ''),
				),
			() => getDuplicate() !== undefined,
		);
	}

	#findDuplicate(items: Array<string>) {
		return items.find((item, index) => items.indexOf(item) !== index);
	}

	#duplicateValue() {
		return this.#findDuplicate(this._entries.map(({ value }) => (value.trim() === '' ? '' : value)));
	}

	// Missing labels are reported by the required rule.
	#duplicateLabel() {
		return this.#findDuplicate(this._entries.map(({ label }) => label.trim()).filter((label) => label !== ''));
	}

	#hasDuplicates() {
		return this.#duplicateValue() !== undefined || this.#duplicateLabel() !== undefined;
	}

	#onAdd() {
		this._entries = [...this._entries, { id: crypto.randomUUID(), value: '', label: '', isNew: true }];
		this.#sorter.setModel(this._entries);
		this.dispatchEvent(new UmbChangeEvent());
		void this.#focusNewEntry();
	}

	#onChange(event: Event, id: string) {
		const { value, label } = event.currentTarget as UmbInputValueWithLabelElement;
		this._entries = this._entries.map((entry) =>
			entry.id === id ? { ...entry, value: value as string, label } : entry,
		);
		this.#sorter.setModel(this._entries);
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onOptionFocusOut() {
		if (this.#hasDuplicates()) super.checkValidity();
	}

	override checkValidity() {
		const rows = Array.from(this.shadowRoot?.querySelectorAll<UmbInputValueWithLabelElement>('.option') ?? []);
		const rowsAreValid = rows.map((row) => row.checkValidity()).every(Boolean);
		return super.checkValidity() && rowsAreValid;
	}

	#onDelete(id: string) {
		this._entries = this._entries.filter((entry) => entry.id !== id);
		this.#sorter.setModel(this._entries);
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #focusNewEntry() {
		await this.updateComplete;
		const options = this.shadowRoot?.querySelectorAll<UmbInputValueWithLabelElement>('.option');
		options?.[options.length - 1]?.focus();
	}

	override getFormElement() {
		return undefined;
	}

	override render() {
		return html`<div id="options">
				${this.#renderHeader()}
				<div id="sorter-wrapper">${this.#renderEntries()}</div>
			</div>
			${this.#renderAddButton()}
			<umb-form-validation-message .for=${this}></umb-form-validation-message>`;
	}

	#renderHeader() {
		if (this._entries.length === 0) return nothing;
		return html`
			<div id="header" aria-hidden="true" inert>
				<span>${this.localize.term('general_label')}</span>
				<span>${this.localize.term('general_value')}</span>
			</div>
		`;
	}

	#renderEntries() {
		return repeat(
			this._entries,
			(entry) => entry.id,
			(entry) => html`
				<umb-input-value-with-label
					class="option"
					data-sort-entry-id=${entry.id}
					.value=${entry.value}
					.label=${entry.label}
					.isNew=${entry.isNew ?? false}
					?readonly=${this.readonly}
					@focusout=${this.#onOptionFocusOut}
					@change=${(event: Event) => this.#onChange(event, entry.id)}
					@delete=${() => this.#onDelete(entry.id)}></umb-input-value-with-label>
			`,
		);
	}

	#renderAddButton() {
		if (this.readonly && this._entries.length > 0) return nothing;
		return html`
			<uui-button
				id="action"
				color="default"
				look="placeholder"
				label=${this.localize.term('general_add')}
				?disabled=${this.readonly}
				@mousedown=${preventFocusLoss}
				@click=${this.#onAdd}></uui-button>
		`;
	}

	static override readonly styles = [
		css`
			#action {
				display: block;
			}

			#options {
				display: grid;
				grid-template-columns: auto 1fr 1fr auto;
				column-gap: var(--uui-size-space-3);
			}

			:host([readonly]) #options {
				grid-template-columns: 1fr 1fr;
			}

			#header,
			#sorter-wrapper {
				display: grid;
				grid-column: 1 / -1;
				grid-template-columns: subgrid;
			}

			#header {
				margin-bottom: var(--uui-size-space-2);
				color: var(--uui-color-text-alt);
				font-size: var(--uui-type-small-size);
			}

			#header span:first-child {
				grid-column: 2;
			}

			:host([readonly]) #header span:first-child {
				grid-column: 1;
			}

			.--umb-sorter-placeholder {
				position: relative;
				visibility: hidden;
			}
			.--umb-sorter-placeholder::after {
				content: '';
				position: absolute;
				inset: 0px;
				border-radius: var(--uui-border-radius);
				border: 1px dashed var(--uui-color-divider-emphasis);
			}
		`,
	];
}

export default UmbInputValuesWithLabelsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-values-with-labels': UmbInputValuesWithLabelsElement;
	}
}
