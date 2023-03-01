import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

import { UmbLitElement } from '@umbraco-cms/element';
import { DataTypePropertyModel } from '@umbraco-cms/backend-api';
import '@tinymce/tinymce-webcomponent';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property()
	value = 'Some default value';

	@property()
	private _config = {
		statusbar: false,
		width: 500,
		height: 500,
	};

	@property({ type: Array<string> })
	private _toolbar: Array<string> = [];

	@property({ type: Array<string> })
	private _plugins: Array<string> = [];

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyModel>) {
		this._config.width = config.find((x) => x.alias === 'width')?.value;
		this._config.height = config.find((x) => x.alias === 'height')?.value;
		this._toolbar = config.find((x) => x.alias === 'toolbar')?.value;
		this._plugins = config.find((x) => x.alias === 'plugins')?.value.map((x: {[key: string]: string}) => x.name);
	}

	render() {
		return html`<div>
			<tinymce-editor
				.config=${this._config}
				menubar="false"
				contextmenu="false"
				resize="false"
				plugins=${this._plugins.join(' ')}
				toolbar=${this._toolbar.join(' ')}
				>${this.value}</tinymce-editor
			>
		</div>`;
	}
}

export default UmbPropertyEditorUITinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce': UmbPropertyEditorUITinyMceElement;
	}
}
