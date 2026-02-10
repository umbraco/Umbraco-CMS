import { UmbExtensionPickerDataSource } from '../picker-data-source/extension.picker-data-source.js';
import type { UmbExtensionPickerDataSourceConfigCollectionModel } from '../picker-data-source/types.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-extension')
export class UmbInputExtensionElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	#dataSource = new UmbExtensionPickerDataSource(this);
	#dataSourceConfig: UmbExtensionPickerDataSourceConfigCollectionModel = [];
	#allowedExtensionTypes: Array<string> | undefined;

	@property({ type: Array, attribute: false })
	set allowedExtensionTypes(value: Array<string> | undefined) {
		this.#allowedExtensionTypes = value;
		this.#dataSourceConfig = value?.length ? [{ alias: 'allowedExtensionTypes', value }] : [];
	}
	get allowedExtensionTypes(): Array<string> | undefined {
		return this.#allowedExtensionTypes;
	}

	@property({ type: Number })
	min = 0;

	@property({ type: Number })
	max = Infinity;

	@property({ type: Array })
	selection: Array<string> = [];

	@property({ type: Boolean, reflect: true })
	readonly = false;

	protected override getFormElement() {
		return undefined;
	}

	#onChange(event: Event) {
		event.stopPropagation();
		// TODO: import correct type for UmbInputEntityDataElement when exported
		const target = event.target as HTMLElement & { selection: Array<string>; value: string | undefined };
		this.selection = target.selection;
		this.value = target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-entity-data
			.dataSourceApi=${this.#dataSource}
			.dataSourceConfig=${this.#dataSourceConfig}
			.value=${this.value}
			.selection=${this.selection}
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
