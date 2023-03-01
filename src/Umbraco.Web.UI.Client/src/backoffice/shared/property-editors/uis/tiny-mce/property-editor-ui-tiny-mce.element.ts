import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

import { UmbLitElement } from '@umbraco-cms/element';
import { DataTypePropertyModel } from '@umbraco-cms/backend-api';
import '@tinymce/tinymce-webcomponent';

declare global {
	interface Window {
		tinyConfig: any;
	}
}

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property()
	value: string;

	@property()
	private _dimensions: { [key: string]: number };

	@property()
	private _styleFormats = [
		{
			title: 'Headers',
			items: [
				{ title: 'Page header', block: 'h2' },
				{ title: 'Section header', block: 'h3' },
				{ title: 'Paragraph header', block: 'h4' },
			],
		},
		{
			title: 'Blocks',
			items: [{ title: 'Normal', block: 'p' }],
		},
		{
			title: 'Containers',
			items: [
				{ title: 'Quote', block: 'blockquote' },
				{ title: 'Code', block: 'code' },
			],
		},
	];

	@property({ type: Array<string> })
	private _toolbar: Array<string> = [];

	@property({ type: Array<string> })
	private _plugins: Array<string> = [];

	@property({ type: Array<string> })
	private _stylesheets: Array<string> = [];

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyModel>) {
		this._dimensions = config.find((x) => x.alias === 'dimensions')?.value as { [key: string]: number };
		this._toolbar = config.find((x) => x.alias === 'toolbar')?.value;
		this._plugins = config.find((x) => x.alias === 'plugins')?.value.map((x: { [key: string]: string }) => x.name);
		this._stylesheets = config.find((x) => x.alias === 'stylesheets')?.value;
	}

	constructor() {
		super();

		this.value = 'A default value';

		window.tinyConfig = {
			statusbar: false,
			menubar: false,
			contextMenu: false,
			resize: false,
			style_formats: this._styleFormats,
		};
	}

	render() {
		return html`<div>
			<tinymce-editor
				config="tinyConfig"
				width=${this._dimensions.width}
				height=${this._dimensions.height}
				plugins=${this._plugins.join(' ')}
				toolbar=${this._toolbar.join(' ')}
				content_css=${this._stylesheets.join(',')}
				.style_formats=${this._styleFormats}
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
