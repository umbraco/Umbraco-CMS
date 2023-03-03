import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { AstNode, Editor, EditorEvent, TinyMCE } from 'tinymce';
import { UmbMediaHelper } from '../../property-editors/uis/tiny-mce/media-helper.service';
import { AcePlugin } from '../../property-editors/uis/tiny-mce/plugins/ace.plugin';
import { LinkPickerPlugin } from '../../property-editors/uis/tiny-mce/plugins/linkpicker.plugin';
import { MacroPlugin } from '../../property-editors/uis/tiny-mce/plugins/macro.plugin';
import { MediaPickerPlugin } from '../../property-editors/uis/tiny-mce/plugins/mediapicker.plugin';
import {
	UmbCurrentUserStore,
	UMB_CURRENT_USER_STORE_CONTEXT_TOKEN,
} from '../../../users/current-user/current-user.store';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import type { UserDetails } from '@umbraco-cms/models';
import { DataTypePropertyModel } from '@umbraco-cms/backend-api';

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
		tinyConfig: { [key: string]: string | number | boolean | object | (() => void) };
		tinymce: TinyMCE;
		Umbraco: any;
	}
}

@customElement('umb-input-tiny-mce')
export class UmbInputTinyMceElement extends FormControlMixin(UmbLitElement) {
	static styles = [UUITextStyles];

	@property()
	configuration: Array<DataTypePropertyModel> = [];

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

	#currentUserStore?: UmbCurrentUserStore;
	modalContext?: UmbModalContext;
	#mediaHelper = new UmbMediaHelper();
	currentUser?: UserDetails;

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.modalContext = instance;
			this.#setTinyConfig();
		});

		this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, (instance) => {
			this.#currentUserStore = instance;
			this.#observeCurrentUser();
		});
	}

	async #observeCurrentUser() {
		if (!this.#currentUserStore) return;

		this.observe(this.#currentUserStore.currentUser, (currentUser) => {
			this.currentUser = currentUser;
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

		// no auto resize when a fixed height is set
		if (!this._dimensions.height) {
			this._plugins.splice(this._plugins.indexOf('autoresize'), 1);
		}
	}

	// TODO => setup runs before rendering, here we can add any custom plugins
	// TODO => fix TinyMCE type definitions
	#setTinyConfig() {
		window.tinyConfig = {
			content_css: false,
			contextMenu: false,
			convert_urls: false,
			menubar: false,
			resize: false,
			//skin: false,
			statusbar: false,
			style_formats: this._styleFormats,
			setup: (editor: Editor) => this.#editorSetup(editor),
		};
	}

	#editorSetup(editor: Editor) {
		// initialise core plugins
		new AcePlugin(editor, this.modalContext);
		new LinkPickerPlugin(editor, this.modalContext, this.configuration);
		new MacroPlugin(editor, this.modalContext);
		new MediaPickerPlugin(editor, this.configuration, this.modalContext, this.currentUser);

		// register custom option maxImageSize
		editor.options.register('maxImageSize', { processor: 'number', default: 500 });

		// If we can not find the insert image/media toolbar button
		// Then we need to add an event listener to the editor
		// That will update native browser drag & drop events
		// To update the icon to show you can NOT drop something into the editor
		if (this._toolbar && !this.#isMediaPickerEnabled()) {
			// Wire up the event listener
			editor.on('dragstart dragend dragover draggesture dragdrop drop drag', (e: EditorEvent<InputEvent>) => {
				e.preventDefault();
				if (e.dataTransfer) {
					e.dataTransfer.effectAllowed = 'none';
					e.dataTransfer.dropEffect = 'none';
				}
				e.stopPropagation();
			});
		}

		editor.addShortcut('Ctrl+S', '', () => this.dispatchEvent(new CustomEvent('rte.shortcut.save')));
		editor.addShortcut('Ctrl+P', '', () => this.dispatchEvent(new CustomEvent('rte.shortcut.saveAndPublish')));

		editor.on('init', () => this.#onInit(editor));
		editor.on('focus', () => this.dispatchEvent(new CustomEvent('umb-rte-focus', { composed: true, bubbles: true })));
		editor.on('blur', () => {
			this.#onChange(editor.getContent());
			this.dispatchEvent(new CustomEvent('umb-rte-blur', { composed: true, bubbles: true }));
		});
		editor.on('Change', () => this.#onChange(editor.getContent()));
		editor.on('Dirty', () => this.#onChange(editor.getContent()));
		editor.on('Keyup', () => this.#onChange(editor.getContent()));
		editor.on('SetContent', () => this.#uploadBlobImages(editor));
		editor.on('ObjectResized', (e) => {
			this.#onResize(e);
			this.#onChange(editor.getContent());
		});
	}

	async #onResize(
		e: EditorEvent<{
			target: HTMLElement;
			width: number;
			height: number;
			origin: string;
		}>
	) {
		const srcAttr = e.target.getAttribute('src');

		if (!srcAttr) {
			return;
		}

		const path = srcAttr.split('?')[0];
		const resizedPath = await this.#mediaHelper.getProcessedImageUrl(path, {
			width: e.width,
			height: e.height,
			mode: 'max',
		});

		e.target.setAttribute('data-mce-src', resizedPath);
	}

	#onInit(editor: Editor) {
		//enable browser based spell checking
		editor.getBody().setAttribute('spellcheck', 'true');

		/** Setup sanitization for preventing injecting arbitrary JavaScript execution in attributes:
		 * https://github.com/advisories/GHSA-w7jx-j77m-wp65
		 * https://github.com/advisories/GHSA-5vm8-hhgr-jcjp
		 */
		const uriAttributesToSanitize = [
			'src',
			'href',
			'data',
			'background',
			'action',
			'formaction',
			'poster',
			'xlink:href',
		];

		const parseUri = (function () {
			// Encapsulated JS logic.
			const safeSvgDataUrlElements = ['img', 'video'];
			const scriptUriRegExp = /((java|vb)script|mhtml):/i;
			// eslint-disable-next-line no-control-regex
			const trimRegExp = /[\s\u0000-\u001F]+/g;

			const isInvalidUri = (uri: string, tagName: string) => {
				if (/^data:image\//i.test(uri)) {
					return safeSvgDataUrlElements.indexOf(tagName) !== -1 && /^data:image\/svg\+xml/i.test(uri);
				} else {
					return /^data:/i.test(uri);
				}
			};

			return function parseUri(uri: string, tagName: string) {
				uri = uri.replace(trimRegExp, '');
				try {
					// Might throw malformed URI sequence
					uri = decodeURIComponent(uri);
				} catch (ex) {
					// Fallback to non UTF-8 decoder
					uri = unescape(uri);
				}

				if (scriptUriRegExp.test(uri)) {
					return;
				}

				if (isInvalidUri(uri, tagName)) {
					return;
				}

				return uri;
			};
		})();

		if (window.Umbraco?.Sys.ServerVariables.umbracoSettings.sanitizeTinyMce) {
			uriAttributesToSanitize.forEach((attribute) => {
				editor.serializer.addAttributeFilter(attribute, (nodes: AstNode[]) => {
					nodes.forEach((node: AstNode) => {
						node.attributes?.forEach((attr) => {
							if (uriAttributesToSanitize.includes(attr.name.toLowerCase())) {
								attr.value = parseUri(attr.value, node.name) ?? '';
							}
						});
					});
				});
			});
		}
	}

	async #uploadBlobImages(editor: Editor) {
		const content = editor.getContent();

		// Upload BLOB images (dragged/pasted ones)
		// find src attribute where value starts with `blob:`
		// search is case-insensitive and allows single or double quotes
		if (content.search(/src=["']blob:.*?["']/gi) !== -1) {
			const data = await editor.uploadImages();
			// Once all images have been uploaded
			data.forEach((item) => {
				// Skip items that failed upload
				if (item.status === false) {
					return;
				}

				// Select img element
				const img = item.element;

				// Get img src
				const imgSrc = img.getAttribute('src');
				const tmpLocation = localStorage.get(`tinymce__${imgSrc}`);

				// Select the img & add new attr which we can search for
				// When its being persisted in RTE property editor
				// To create a media item & delete this tmp one etc
				editor.dom.setAttrib(img, 'data-tmpimg', tmpLocation);

				// Resize the image to the max size configured
				// NOTE: no imagesrc passed into func as the src is blob://...
				// We will append ImageResizing Querystrings on perist to DB with node save
				this.#mediaHelper.sizeImageInEditor(editor, img);
			});

			// Get all img where src starts with blob: AND does NOT have a data=tmpimg attribute
			// This is most likely seen as a duplicate image that has already been uploaded
			// editor.uploadImages() does not give us any indiciation that the image been uploaded already
			const blobImageWithNoTmpImgAttribute = editor.dom.select('img[src^="blob:"]:not([data-tmpimg])');

			//For each of these selected items
			blobImageWithNoTmpImgAttribute.forEach((imageElement) => {
				const blobSrcUri = editor.dom.getAttrib(imageElement, 'src');

				// Find the same image uploaded (Should be in LocalStorage)
				// May already exist in the editor as duplicate image
				// OR added to the RTE, deleted & re-added again
				// So lets fetch the tempurl out of localstorage for that blob URI item

				const tmpLocation = localStorage.get(`tinymce__${blobSrcUri}`);
				if (tmpLocation) {
					this.#mediaHelper.sizeImageInEditor(editor, imageElement);
					editor.dom.setAttrib(imageElement, 'data-tmpimg', tmpLocation);
				}
			});
		}

		if (window.Umbraco?.Sys.ServerVariables.umbracoSettings.sanitizeTinyMce) {
			/** prevent injecting arbitrary JavaScript execution in on-attributes. */
			const allNodes = Array.from(editor.dom.doc.getElementsByTagName('*'));
			allNodes.forEach((node) => {
				for (let i = 0; i < node.attributes.length; i++) {
					if (node.attributes[i].name.startsWith('on')) {
						node.removeAttribute(node.attributes[i].name);
					}
				}
			});
		}
	}

	#onChange(value: string) {
		super.value = value;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#isMediaPickerEnabled() {
		return this._toolbar.includes('umbmediapicker');
	}

	render() {
		return html` <tinymce-editor
			config="tinyConfig"
			width=${ifDefined(this._dimensions?.width)}
			height=${ifDefined(this._dimensions?.height)}
			plugins=${this._plugins.join(' ')}
			toolbar=${this._toolbar.join(' ')}
			content_css=${this._stylesheets.join(',')}
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
