import type { UmbInputUploadFieldElement } from '../../components/input-upload-field/input-upload-field.element.js';
import type { UmbMediaValueType } from './types.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

import '../../components/input-upload-field/input-upload-field.element.js';

/**
 * @element umb-property-editor-ui-upload-field
 */
@customElement('umb-property-editor-ui-upload-field')
export class UmbPropertyEditorUIUploadFieldElement
	extends UmbFormControlMixin<UmbMediaValueType, typeof UmbLitElement>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	@state()
	private _fileExtensions?: Array<string>;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		this._fileExtensions = config.getValueByAlias<Array<string>>('fileExtensions');
		if (this._fileExtensions?.length) {
			this._fileExtensions = this._fileExtensions.map((ext) =>
				ext.startsWith('.') ? ext : ext.includes('/') ? ext : `.${ext}`,
			);
		}
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-upload-field')!);
	}

	#onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputUploadFieldElement).value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-upload-field
				.allowedFileExtensions=${this._fileExtensions}
				.value=${this.value}
				@change=${this.#onChange}>
			</umb-input-upload-field>
		`;
	}
}

export default UmbPropertyEditorUIUploadFieldElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-upload-field': UmbPropertyEditorUIUploadFieldElement;
	}
}
