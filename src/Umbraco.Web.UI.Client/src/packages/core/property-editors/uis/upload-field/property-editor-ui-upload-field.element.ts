import { UmbInputUploadFieldElement } from '../../../components/input-upload-field/input-upload-field.element.js';
import { html , customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import type { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-upload-field
 */
@customElement('umb-property-editor-ui-upload-field')
export class UmbPropertyEditorUIUploadFieldElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		this._fileExtensions = config.getValueByAlias('fileExtensions');
		this._multiple = config.getValueByAlias('multiple');
	}

	@state()
	private _fileExtensions?: Array<string>;

	@state()
	private _multiple?: boolean;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as unknown as UmbInputUploadFieldElement).value as string;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-upload-field
			@change="${this._onChange}"
			?multiple="${this._multiple}"
			.fileExtensions="${this._fileExtensions}"></umb-input-upload-field>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIUploadFieldElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-upload-field': UmbPropertyEditorUIUploadFieldElement;
	}
}
