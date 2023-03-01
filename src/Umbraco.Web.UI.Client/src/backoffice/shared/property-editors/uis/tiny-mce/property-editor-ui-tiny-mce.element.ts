import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { LinkPickerPlugin } from './plugins/linkpicker.plugin';
import { AcePlugin } from './plugins/ace.plugin';
import { UmbLitElement } from '@umbraco-cms/element';
import { DataTypePropertyModel } from '@umbraco-cms/backend-api';

/// TINY MCE
import '@tinymce/tinymce-webcomponent';
// import 'tinymce';
// /* Default icons are required. After that, import custom icons if applicable */
// import 'tinymce/icons/default';

// /* Required TinyMCE components */
// import 'tinymce/themes/silver';
// import 'tinymce/models/dom';

// /* Import a skin (can be a custom skin instead of the default) */
// import 'tinymce/skins/ui/oxide/skin.css';

// /* Import plugins */
// import 'tinymce/plugins/advlist';
// import 'tinymce/plugins/code';
// import 'tinymce/plugins/emoticons';
// import 'tinymce/plugins/emoticons/js/emojis';
// import 'tinymce/plugins/link';
// import 'tinymce/plugins/lists';
// import 'tinymce/plugins/table';

import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

declare global {
	interface Window {
		tinyConfig: any;
		tinymce: any;
	}
}

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property()
	value?: string;

	@property()
	private _dimensions?: { [key: string]: number };

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

	configuration?: Array<DataTypePropertyModel> = [];

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyModel>) {
		this.configuration = config;
		this._dimensions = config.find((x) => x.alias === 'dimensions')?.value as { [key: string]: number };
		this._toolbar = config.find((x) => x.alias === 'toolbar')?.value;
		this._plugins = config.find((x) => x.alias === 'plugins')?.value.map((x: { [key: string]: string }) => x.name);
		this._stylesheets = config.find((x) => x.alias === 'stylesheets')?.value;
	}

	// TODO => setup runs before rendering, here we can add any custom plugins
	// TODO => fix TinyMCE type definitions
	#setTinyConfig() {
		window.tinyConfig = {
			statusbar: false,
			menubar: false,
			contextMenu: false,
			resize: false,
			style_formats: this._styleFormats,
			setup: (editor: any) => {
				new AcePlugin(editor, this.modalService);
				new LinkPickerPlugin(editor, this.modalService, this.configuration);

				// TODO => the editor frame catches Ctrl+S and handles it with the system save dialog
				// - we want to handle it in the content controller, so we'll emit an event instead
				editor.addShortcut('Ctrl+S', '', () => window.dispatchEvent(new CustomEvent('rte.shortcut.save')));
				editor.addShortcut('Ctrl+P', '', () => window.dispatchEvent(new CustomEvent('rte.shortcut.saveAndPublish')));
			},
		};
	}

	modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.modalService = instance;
			this.#setTinyConfig();
		});
	}

	render() {
		return html`<div>
			<tinymce-editor
				config="tinyConfig"
				width=${this._dimensions?.width ?? 600}
				height=${this._dimensions?.height ?? 400}
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
