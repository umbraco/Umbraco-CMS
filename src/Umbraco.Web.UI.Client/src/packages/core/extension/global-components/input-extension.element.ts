import { UmbExtensionPickerDataSource } from '../picker-data-source/extension.picker-data-source.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-extension')
export class UmbInputExtensionElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#pickerDataSource = new UmbExtensionPickerDataSource(this);

	@property({ type: Number })
	min = 0;

	@property({ type: Number })
	max = Infinity;

	@property({ type: Array })
	public set selection(uniques: Array<string>) {
		this.#selection = uniques;
		super.value = uniques.length > 0 ? uniques.join(',') : undefined;
	}
	public get selection(): Array<string> {
		return this.#selection;
	}
	#selection: Array<string> = [];

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.#selection = splitStringToArray(selectionString);
		super.value = selectionString;
	}
	public override get value(): string | undefined {
		return this.#selection.length > 0 ? this.#selection.join(',') : undefined;
	}

	@property({ type: Boolean, reflect: true })
	readonly = false;

	protected override getFormElement() {
		return undefined;
	}

	#onChange(event: Event) {
		event.stopPropagation();
		const target = event.target as HTMLElement & { selection: Array<string>; value: string | undefined };
		this.#selection = target.selection;
		super.value = target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-entity-data
			.selection=${this.#selection}
			.dataSourceApi=${this.#pickerDataSource}
			.min=${this.min}
			.max=${this.max}
			?readonly=${this.readonly}
			@change=${this.#onChange}></umb-input-entity-data>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-extension': UmbInputExtensionElement;
	}
}
