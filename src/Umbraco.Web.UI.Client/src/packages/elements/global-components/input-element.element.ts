import { UmbElementTreePickerDataSource } from '../picker-data-source/element-tree.picker-data-source.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-element')
export class UmbInputElementElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
	undefined,
) {
	#dataSourceApi = new UmbElementTreePickerDataSource(this);

	#dataSourceConfig: UmbConfigCollectionModel = [];

	#folderOnly = false;

	@property({ type: Boolean })
	set folderOnly(value: boolean) {
		this.#folderOnly = value;
		this.#dataSourceConfig = [{ alias: 'folderOnly', value }];
	}
	get folderOnly(): boolean {
		return this.#folderOnly;
	}

	@property({ type: Number })
	min = 0;

	@property({ type: String, attribute: 'min-message' })
	minMessage?: string;

	@property({ type: Number })
	max = Infinity;

	@property({ type: String, attribute: 'max-message' })
	maxMessage?: string;

	@property({ type: Array })
	selection: Array<string> = [];

	@property({ type: Boolean, reflect: true })
	readonly = false;

	protected override getFormElement() {
		return undefined;
	}

	#onChange(event: CustomEvent & { target: { selection: Array<string>; value: string } }) {
		this.selection = event.target.selection;
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-entity-data
				.dataSourceApi=${this.#dataSourceApi}
				.dataSourceConfig=${this.#dataSourceConfig}
				.value=${this.value}
				.selection=${this.selection}
				.min=${this.min}
				.minMessage=${this.minMessage}
				.max=${this.max}
				.maxMessage=${this.maxMessage}
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
