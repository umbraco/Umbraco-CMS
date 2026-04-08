import { UmbElementTreePickerDataSource } from '../picker-data-source/element-tree.picker-data-source.js';
import { html, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';

@customElement('umb-input-element')
export class UmbInputElementElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
	undefined,
) {
	#dataSourceApi = new UmbElementTreePickerDataSource(this);

	@property({ type: Boolean })
	folderOnly = false;

	@property({ type: Number })
	min = 0;

	@property({ type: String, attribute: 'min-message' })
	minMessage?: string;

	@property({ type: Number })
	max = Infinity;

	@property({ type: String, attribute: 'max-message' })
	maxMessage?: string;

	#selection: Array<string> = [];

	@property({ type: Array })
	public set selection(ids: Array<string>) {
		this.#selection = ids;
		super.value = ids.length > 0 ? ids.join(',') : undefined;
	}
	public get selection(): Array<string> {
		return this.#selection;
	}

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.#selection = splitStringToArray(selectionString);
		super.value = selectionString; // Call the parent setter to ensure the value change is triggered in the FormControlMixin.
	}
	public override get value(): string | undefined {
		return this.#selection.length > 0 ? this.#selection.join(',') : undefined;
	}

	@property({ type: Boolean, reflect: true })
	readonly = false;

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-entity-data')!);
	}

	#onChange(event: CustomEvent & { target: { selection: Array<string>; value: string } }) {
		this.#selection = event.target.selection;
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		const dataSourceConfig = [
			{ alias: 'folderOnly', value: this.folderOnly },
			{ alias: 'startNode', value: this.startNode },
		];
		return html`
			<umb-input-entity-data
				min-message=${ifDefined(this.minMessage)}
				max-message=${ifDefined(this.maxMessage)}
				.dataSourceConfig=${dataSourceConfig}
				.dataSourceApi=${this.#dataSourceApi}
				.value=${this.value}
				.selection=${this.selection}
				.min=${this.min}
				.max=${this.max}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-entity-data>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-element': UmbInputElementElement;
	}
}
