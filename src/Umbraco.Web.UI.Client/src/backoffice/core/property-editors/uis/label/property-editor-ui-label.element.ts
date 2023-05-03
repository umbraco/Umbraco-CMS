import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

type umbracoDataValueType = 'STRING' | 'DECIMAL' | 'DATE/TIME' | 'TIME' | 'INTEGER' | 'BIG INTEGER' | 'LONG STRING';

/**
 * @element umb-property-editor-ui-label
 */
@customElement('umb-property-editor-ui-label')
export class UmbPropertyEditorUILabelElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property()
	description = '';

	@state()
	private _dvt?: umbracoDataValueType;

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		const dvt = config.find((x) => x.alias === 'umbracoDataValueType');
		if (dvt) this._dvt = dvt.value as umbracoDataValueType;
	}

	render() {
		return html`${this.value}`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUILabelElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-label': UmbPropertyEditorUILabelElement;
	}
}
