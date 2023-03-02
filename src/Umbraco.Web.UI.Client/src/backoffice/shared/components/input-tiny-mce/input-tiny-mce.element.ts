import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { AcePlugin } from '../../property-editors/uis/tiny-mce/plugins/ace.plugin';
import { LinkPickerPlugin } from '../../property-editors/uis/tiny-mce/plugins/linkpicker.plugin';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

/// TINY MCE
// import 'tinymce';
import '@tinymce/tinymce-webcomponent';

// /* Default icons are required. After that, import custom icons if applicable */
// import 'tinymce/icons/default';

// /* Required TinyMCE components */
// import 'tinymce/themes/silver';
// import 'tinymce/models/dom';

// /* Import a skin (can be a custom skin instead of the default) */
// import 'tinymce/skins/ui/oxide/skin.shadowdom.css';

// /* content UI CSS is required */
// import contentUiSkinCss from 'tinymce/skins/ui/oxide/content.css';

// /* The default content CSS can be changed or replaced with appropriate CSS for the editor content. */
// import contentCss from 'tinymce/skins/content/default/content.css';

// /* Import plugins */
// import 'tinymce/plugins/advlist';
// import 'tinymce/plugins/anchor';
// import 'tinymce/plugins/autolink';
// import 'tinymce/plugins/charmap';
// import 'tinymce/plugins/directionality';
// import 'tinymce/plugins/lists';
// import 'tinymce/plugins/searchreplace';
// import 'tinymce/plugins/table';

declare global {
	interface Window {
		tinyConfig: any;
		tinymce: any;
	}
}

@customElement('umb-input-tiny-mce')
export class UmbInputTinyMceElement extends FormControlMixin(UmbLitElement) {
	static styles = [UUITextStyles];

	@property()
	configuration: Array<any> = [];

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

	// @property({ type: String })
	// private _contentStyle: string = contentUiSkinCss.toString() + '\n' + contentCss.toString();

	modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.modalService = instance;
			this.#setTinyConfig();
		});
	}

	connectedCallback() {
		super.connectedCallback();

		this._dimensions = this.configuration.find((x) => x.alias === 'dimensions')?.value as { [key: string]: number };
		this._toolbar = this.configuration.find((x) => x.alias === 'toolbar')?.value;
		this._plugins = this.configuration
			.find((x) => x.alias === 'plugins')
			?.value.map((x: { [key: string]: string }) => x.name);
		this._stylesheets = this.configuration.find((x) => x.alias === 'stylesheets')?.value;
	}

	// TODO => setup runs before rendering, here we can add any custom plugins
	// TODO => fix TinyMCE type definitions
	#setTinyConfig() {
		window.tinyConfig = {
			//content_css: false,
			contextMenu: false,
			convert_urls: false,
			menubar: false,
			resize: false,
			//skin: false,
			statusbar: false,
			style_formats: this._styleFormats,

			setup: (editor: any) => {
				new AcePlugin(editor, this.modalService);
				new LinkPickerPlugin(editor, this.modalService, this.configuration);

				// TODO => the editor frame catches Ctrl+S and handles it with the system save dialog
				// - we want to handle it in the content controller, so we'll emit an event instead
				editor.addShortcut('Ctrl+S', '', () => this.dispatchEvent(new CustomEvent('rte.shortcut.save')));
				editor.addShortcut('Ctrl+P', '', () => this.dispatchEvent(new CustomEvent('rte.shortcut.saveAndPublish')));

				editor.on('Change', () => this.#onChange(editor.getContent()));
			},
		};
	}

	protected getFormElement() {
		return undefined;
	}

	#onChange(value: string) {
		super.value = value;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html` <tinymce-editor
			config="tinyConfig"
			width=${ifDefined(this._dimensions?.width)}
			height=${ifDefined(this._dimensions?.height)}
			plugins=${this._plugins.join(' ')}
			toolbar=${this._toolbar.join(' ')}
			>${this.value}</tinymce-editor
		>`;
	}
}

export default UmbInputTinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tiny-mce': UmbInputTinyMceElement;
	}
}
