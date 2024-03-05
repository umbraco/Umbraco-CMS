import type { UmbInputMediaElement } from '../../components/input-media/input-media.element.js';
import '../../components/input-media/input-media.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

/**
 * @element umb-property-editor-ui-media-picker
 */
@customElement('umb-property-editor-ui-media-picker')
export class UmbPropertyEditorUIMediaPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Array })
	public value?: Array<string> | string;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const validationLimit = config?.getByAlias('validationLimit');
		if (!validationLimit) return;

		const minMax: Record<string, number> = validationLimit.value;

		this._limitMin = minMax.min ?? 0;
		this._limitMax = minMax.max ?? Infinity;
	}
	public get config() {
		return undefined;
	}

	@state()
	private _limitMin: number = 0;
	@state()
	private _limitMax: number = Infinity;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputMediaElement).selectedIds;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`
			<umb-input-media
				@change=${this._onChange}
				.selectedIds=${this.value ? (Array.isArray(this.value) ? this.value : splitStringToArray(this.value)) : []}
				.min=${this._limitMin}
				.max=${this._limitMax}
				>Add</umb-input-media
			>
		`;
	}
}

export default UmbPropertyEditorUIMediaPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-picker': UmbPropertyEditorUIMediaPickerElement;
	}
}
