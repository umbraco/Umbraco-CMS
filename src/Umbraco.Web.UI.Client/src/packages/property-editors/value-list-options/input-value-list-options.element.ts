import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * An option as it is stored in the configuration of a value list: the value, and the label when it has one.
 */
export interface UmbValueListOptionConfiguration {
	value: string;
	label?: string;
}

interface UmbValueListOptionEntry {
	id: string;
	value: string;
	label: string;
}

/**
 * @element umb-input-value-list-options
 */
@customElement('umb-input-value-list-options')
export class UmbInputValueListOptionsElement extends UmbFormControlMixin<undefined | string, typeof UmbLitElement>(
	UmbLitElement,
	undefined,
) {
	#sorter = new UmbSorterController<UmbValueListOptionEntry>(this, {
		getUniqueOfElement: (element) => element.getAttribute('data-sort-entry-id'),
		getUniqueOfModel: (entry) => entry.id,
		identifier: 'Umb.SorterIdentifier.ValueListOptions',
		itemSelector: '.option',
		containerSelector: '#sorter-wrapper',
		onChange: ({ model }) => {
			this._entries = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	@state()
	private _entries: Array<UmbValueListOptionEntry> = [];

	/**
	 * The options, each with a value and optionally a label.
	 * @type {Array<UmbValueListOptionConfiguration>}
	 */
	@property({ type: Array })
	public set items(items: Array<UmbValueListOptionConfiguration> | undefined) {
		this._entries = (items ?? []).map(({ value, label }) => ({
			id: crypto.randomUUID(),
			value,
			label: label ?? '',
		}));
		this.value = this._entries.length > 0 ? 'some value' : '';
		this.#sorter.setModel(this._entries);
	}
	public get items(): Array<UmbValueListOptionConfiguration> {
		return this._entries.map(({ value, label }) => {
			const trimmedLabel = label.trim();
			return trimmedLabel ? { value, label: trimmedLabel } : { value };
		});
	}

	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public set disabled(value: boolean) {
		this.#disabled = value;
		if (value) this.#sorter.disable();
	}
	public get disabled() {
		return this.#disabled;
	}
	#disabled = false;

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

		this.addValidator(
			'valueMissing',
			() => this.localize.term('validation_itemMissingValueMessage', this.#indexOfEmptyValue() + 1),
			() => this.#indexOfEmptyValue() !== -1,
		);
		this.addValidator(
			'customError',
			() => this.localize.term('validation_valueMustBeUnique', this.#duplicateValues()[0]),
			() => this.#duplicateValues().length > 0,
		);
	}

	get #locked() {
		return this.disabled || this.readonly;
	}

	#indexOfEmptyValue() {
		return this._entries.findIndex((entry) => entry.value.trim() === '');
	}

	#duplicateValues() {
		const seen = new Set<string>();
		const duplicates = new Set<string>();
		for (const { value } of this._entries) {
			if (seen.has(value)) duplicates.add(value);
			seen.add(value);
		}
		return Array.from(duplicates);
	}

	#onAdd() {
		this._entries = [...this._entries, { id: crypto.randomUUID(), value: '', label: '' }];
		this.#sorter.setModel(this._entries);
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
		this.#focusNewEntry();
	}

	#onValueInput(event: UUIInputEvent, id: string) {
		event.stopPropagation();
		this.#updateEntry(id, { value: event.target.value as string });
	}

	#onLabelInput(event: UUIInputEvent, id: string) {
		event.stopPropagation();
		this.#updateEntry(id, { label: event.target.value as string });
	}

	#updateEntry(id: string, changes: Partial<UmbValueListOptionEntry>) {
		this._entries = this._entries.map((entry) => (entry.id === id ? { ...entry, ...changes } : entry));
		this.#sorter.setModel(this._entries);
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onDelete(id: string) {
		this._entries = this._entries.filter((entry) => entry.id !== id);
		this.#sorter.setModel(this._entries);
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #focusNewEntry() {
		await this.updateComplete;
		const inputs = this.shadowRoot?.querySelectorAll<UUIInputElement>('.option .value');
		inputs?.[inputs.length - 1]?.focus();
	}

	override getFormElement() {
		return undefined;
	}

	override render() {
		return html`<div id="sorter-wrapper">${this.#renderEntries()}</div>
			${this.#renderAddButton()}`;
	}

	#renderEntries() {
		const duplicates = this.#duplicateValues();
		return repeat(
			this._entries,
			(entry) => entry.id,
			(entry) => html`
				<div class="option" data-sort-entry-id=${entry.id}>
					${this.#locked ? nothing : html`<uui-icon name="icon-grip" class="handle"></uui-icon>`}
					<uui-input
						class="value"
						label=${this.localize.term('general_value')}
						placeholder=${this.localize.term('general_value')}
						.value=${entry.value}
						?error=${duplicates.includes(entry.value) || entry.value.trim() === ''}
						?disabled=${this.disabled}
						?readonly=${this.readonly}
						@input=${(event: UUIInputEvent) => this.#onValueInput(event, entry.id)}></uui-input>
					<uui-input
						class="label"
						label=${this.localize.term('placeholders_label')}
						placeholder=${this.localize.term('placeholders_label')}
						.value=${entry.label}
						?disabled=${this.disabled}
						?readonly=${this.readonly}
						@input=${(event: UUIInputEvent) => this.#onLabelInput(event, entry.id)}></uui-input>
					${this.#locked
						? nothing
						: html`<uui-button
								class="delete"
								compact
								look="outline"
								label=${this.localize.term('actions_delete')}
								@click=${() => this.#onDelete(entry.id)}>
								<uui-icon name="icon-trash"></uui-icon>
							</uui-button>`}
				</div>
			`,
		);
	}

	#renderAddButton() {
		if (this.#locked && this._entries.length > 0) return nothing;
		return html`
			<uui-button
				id="action"
				color="default"
				look="placeholder"
				label=${this.localize.term('general_add')}
				?disabled=${this.#locked}
				@click=${this.#onAdd}></uui-button>
		`;
	}

	static override styles = [
		css`
			#action {
				display: block;
			}

			.option {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
				margin-bottom: var(--uui-size-space-3);
			}

			.option uui-input {
				flex: 1;
			}

			.handle {
				cursor: grab;
			}

			.handle:active {
				cursor: grabbing;
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

export default UmbInputValueListOptionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-value-list-options': UmbInputValueListOptionsElement;
	}
}
