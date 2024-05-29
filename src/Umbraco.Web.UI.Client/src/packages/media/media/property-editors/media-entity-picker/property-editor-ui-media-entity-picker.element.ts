import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-property-editor-ui-media-entity-picker')
export class UmbPropertyEditorUIMediaEntityPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#min: number = 0;
	#max: number = Infinity;

	@property({ attribute: false })
	public set value(value: string | null | undefined) {
		this.#selection = value ? (Array.isArray(value) ? value : splitStringToArray(value)) : [];
	}
	public get value() {
		return this.#selection.length > 0 ? this.#selection.join(',') : null;
	}

	#selection: Array<string> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this.#min = minMax?.min ?? 0;
		this.#max = minMax?.max ?? Infinity;
	}
	public get config() {
		return undefined;
	}

	#onChange(event: CustomEvent & { target: UmbInputMediaElement }) {
		this.value = event.target.selection?.join(',') ?? null;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-media
				.min=${this.#min}
				.max=${this.#max}
				.selection=${this.#selection}
				@change=${this.#onChange}></umb-input-media>
		`;
	}
}

export default UmbPropertyEditorUIMediaEntityPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-entity-picker': UmbPropertyEditorUIMediaEntityPickerElement;
	}
}
