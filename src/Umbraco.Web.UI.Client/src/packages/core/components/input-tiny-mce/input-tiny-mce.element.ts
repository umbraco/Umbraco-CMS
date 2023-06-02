import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import tinymce, { AstNode, Editor, EditorEvent } from 'tinymce';
import { firstValueFrom } from 'rxjs';
import {
	UmbCurrentUserStore,
	UMB_CURRENT_USER_STORE_CONTEXT_TOKEN,
} from '../../../users/current-user/current-user.store';
import type { UmbLoggedInUser } from '../../../users/current-user/types';
import { availableLanguages } from './input-tiny-mce.languages';
import {
	TinyMcePluginArguments,
	UmbTinyMcePluginBase,
	ManifestTinyMcePlugin,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';
import { UmbMediaHelper } from '@umbraco-cms/backoffice/utils';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { ClassConstructor, hasDefaultExport, loadExtension } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

// TODO => integrate macro picker, update stylesheet fetch when backend CLI exists (ref tinymce.service.js in existing backoffice)
@customElement('umb-input-tiny-mce')
export class UmbInputTinyMceElement extends FormControlMixin(UmbLitElement) {
	@property()
	configuration?: UmbDataTypePropertyCollection;

	// TODO => create interface when we know what shape that will take
	// TinyMCE provides the EditorOptions interface, but all props are required
	@state()
	private _configObject: Record<string, any> = {};

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

	//These are absolutely required in order for the macros to render inline
	//we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
	#extendedValidElements =
		'@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align],span[id|class|style|lang],figure,figcaption';

	// If no config provided, fallback to these sensible defaults
	#fallbackConfig = {
		toolbar: [
			'ace',
			'styles',
			'bold',
			'italic',
			'alignleft',
			'aligncenter',
			'alignright',
			'bullist',
			'numlist',
			'outdent',
			'indent',
			'link',
			'umbmediapicker',
			'umbmacro',
			'umbembeddialog',
		],
		mode: 'classic',
		stylesheets: [],
		maxImageSize: 500,
	};

	#currentUserStore?: UmbCurrentUserStore;
	modalContext!: UmbModalContext;
	#mediaHelper = new UmbMediaHelper();
	#currentUser?: UmbLoggedInUser;
	#plugins: Array<new (args: TinyMcePluginArguments) => UmbTinyMcePluginBase> = [];

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (modalContext) => {
			this.modalContext = modalContext;
		});

		this.consumeContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, (currentUserStore) => {
			this.#currentUserStore = currentUserStore;
			this.#observeCurrentUser();
		});
	}

	async #observeCurrentUser() {
		if (!this.#currentUserStore) return;

		this.observe(this.#currentUserStore.currentUser, (currentUser: UmbLoggedInUser | undefined) => {
			this.#currentUser = currentUser;
		});
	}

	async connectedCallback() {
		super.connectedCallback();

		// create an object by merging the configuration onto the fallback config
		Object.assign(
			this._configObject,
			this.#fallbackConfig,
			this.configuration ? this.configuration?.toObject() : {}
		);

		// no auto resize when a fixed height is set
		if (!this._configObject.dimensions?.height) {
			this._configObject.plugins ??= [];
			this._configObject.plugins.splice(this._configObject.plugins.indexOf('autoresize'), 1);
		}

		await this.#loadPlugins();
		this.#setTinyConfig();
	}

	/**
	 * Load all custom plugins - need to split loading and instantiating as these
	 * need the editor instance as a ctor argument. If we load them in the editor
	 * setup method, the asynchronous nature means the editor is loaded before
	 * the plugins are ready and so are not associated with the editor.
	 */
	async #loadPlugins() {
		const observable = umbExtensionsRegistry?.extensionsOfType('tinyMcePlugin');
		const plugins = (await firstValueFrom(observable)) as ManifestTinyMcePlugin[];

		for (const plugin of plugins) {
			const module = await loadExtension(plugin);
			if (hasDefaultExport<ClassConstructor<UmbTinyMcePluginBase>>(module)) {
				this.#plugins.push(module.default);
			}
		}
	}

	#setTinyConfig() {
		const target = document.createElement('div');
		target.id = 'editor';
		this.shadowRoot?.appendChild(target);

		// set the default values that will not be modified via configuration
		const tinyConfig: Record<string, any> = {
			autoresize_bottom_margin: 10,
			base_url: '/tinymce',
			body_class: 'umb-rte',
			//see https://www.tiny.cloud/docs/tinymce/6/editor-important-options/#cache_suffix
			cache_suffix: '?umb__rnd=' + window.Umbraco?.Sys.ServerVariables.application.cacheBuster,
			contextMenu: false,
			inline_boundaries_selector: 'a[href],code,.mce-annotation,.umb-embed-holder,.umb-macro-holder',
			menubar: false,
			paste_remove_styles_if_webkit: true,
			paste_preprocess: (_: Editor, args: { content: string }) => this.#cleanupPasteData(args),
			relative_urls: false,
			resize: false,
			target,
			statusbar: false,
			setup: (editor: Editor) => this.#editorSetup(editor),
		};

		// extend with configuration values
		Object.assign(tinyConfig, {
			content_css: this._configObject.stylesheets.join(','),
			extended_valid_elements: this.#extendedValidElements,
			height: this._configObject.height ?? 500,
			invalid_elements: this._configObject.invalidElements,
			plugins: this._configObject.plugins.map((x: any) => x.name),
			toolbar: this._configObject.toolbar.join(' '),
			style_formats: this._styleFormats,
			valid_elements: this._configObject.validElements,
			width: this._configObject.width,
		});

		// Need to check if we are allowed to UPLOAD images
		// This is done by checking if the insert image toolbar button is available
		if (this.#isMediaPickerEnabled()) {
			Object.assign(tinyConfig, {
				// Update the TinyMCE Config object to allow pasting
				images_upload_handler: this.#uploadImageHandler,
				automatic_uploads: false,
				images_replace_blob_uris: false,
				// This allows images to be pasted in & stored as Base64 until they get uploaded to server
				paste_data_images: true,
			});
		}

		this.#setLanguage(tinyConfig);

		tinymce.init(tinyConfig);
	}

	#cleanupPasteData(args: { content: string }) {
		// Remove spans
		args.content = args.content.replace(/<\s*span[^>]*>(.*?)<\s*\/\s*span>/g, '$1');
		// Convert b to strong.
		args.content = args.content.replace(/<\s*b([^>]*)>(.*?)<\s*\/\s*b([^>]*)>/g, '<strong$1>$2</strong$3>');
		// convert i to em
		args.content = args.content.replace(/<\s*i([^>]*)>(.*?)<\s*\/\s*i([^>]*)>/g, '<em$1>$2</em$3>');
	}

	// TODO => arg types
	#uploadImageHandler(blobInfo: any, progress: any) {
		return new Promise((resolve, reject) => {
			const xhr = new XMLHttpRequest();
			xhr.open('POST', window.Umbraco?.Sys.ServerVariables.umbracoUrls.tinyMceApiBaseUrl + 'UploadImage');

			xhr.onloadstart = () => this.dispatchEvent(new CustomEvent('rte.file.uploading'));

			xhr.onloadend = () => this.dispatchEvent(new CustomEvent('rte.file.uploaded'));

			xhr.upload.onprogress = (e) => progress((e.loaded / e.total) * 100);

			xhr.onerror = () => reject('Image upload failed due to a XHR Transport error. Code: ' + xhr.status);

			xhr.onload = () => {
				if (xhr.status < 200 || xhr.status >= 300) {
					reject('HTTP Error: ' + xhr.status);
					return;
				}

				// TODO => confirm this is required given no more Angular handling XHR/HTTP
				const data = xhr.responseText.split('\n');

				if (data.length <= 1) {
					reject('Unrecognized text string: ' + data);
					return;
				}

				let json: { [key: string]: string } = {};

				try {
					json = JSON.parse(data[1]);
				} catch (e: any) {
					reject('Invalid JSON: ' + data + ' - ' + e.message);
					return;
				}

				if (!json || typeof json.tmpLocation !== 'string') {
					reject('Invalid JSON: ' + data);
					return;
				}

				// Put temp location into localstorage (used to update the img with data-tmpimg later on)
				localStorage.set(`tinymce__${blobInfo.blobUri()}`, json.tmpLocation);

				// We set the img src url to be the same as we started
				// The Blob URI is stored in TinyMce's cache
				// so the img still shows in the editor
				resolve(blobInfo.blobUri());
			};

			const formData = new FormData();
			formData.append('file', blobInfo.blob(), blobInfo.blob().name);

			xhr.send(formData);
		});
	}

	/**
	 * Sets the language to use for TinyMCE */
	#setLanguage(tinyConfig: Record<string, any>) {
		const localeId = this.#currentUser?.language;
		//try matching the language using full locale format
		let languageMatch = availableLanguages.find((x) => x.toLowerCase() === localeId);

		//if no matches, try matching using only the language
		if (!languageMatch) {
			const localeParts = localeId?.split('_');
			if (localeParts) {
				languageMatch = availableLanguages.find((x) => x === localeParts[0]);
			}
		}

		// only set if language exists, will fall back to tiny default
		if (languageMatch) {
			tinyConfig.language = languageMatch;
		}
	}

	#editorSetup(editor: Editor) {
		editor.suffix = '.min';

		// register custom option maxImageSize
		editor.options.register('maxImageSize', { processor: 'number', default: this.#fallbackConfig.maxImageSize });

		// instantiate plugins - these are already loaded in this.#loadPlugins
		// to ensure they are available before setting up the editor.
		// Plugins require a reference to the current editor as a param, so can not
		// be instantiated until we have an editor
		for (const plugin of this.#plugins) {
			new plugin({ host: this, editor, configuration: this.configuration });
		}

		// define keyboard shortcuts
		editor.addShortcut('Ctrl+S', '', () =>
			this.dispatchEvent(new CustomEvent('rte.shortcut.save', { composed: true, bubbles: true }))
		);

		editor.addShortcut('Ctrl+P', '', () =>
			this.dispatchEvent(new CustomEvent('rte.shortcut.saveAndPublish', { composed: true, bubbles: true }))
		);

		// bind editor events
		editor.on('init', () => this.#onInit(editor));
		editor.on('Change', () => this.#onChange(editor.getContent()));
		editor.on('Dirty', () => this.#onChange(editor.getContent()));
		editor.on('Keyup', () => this.#onChange(editor.getContent()));
		editor.on('SetContent', () => this.#mediaHelper.uploadBlobImages(editor));

		editor.on('focus', () => this.dispatchEvent(new CustomEvent('umb-rte-focus', { composed: true, bubbles: true })));

		editor.on('blur', () => {
			this.#onChange(editor.getContent());
			this.dispatchEvent(new CustomEvent('umb-rte-blur', { composed: true, bubbles: true }));
		});

		editor.on('ObjectResized', (e) => {
			this.#mediaHelper.onResize(e);
			this.#onChange(editor.getContent());
		});

		editor.on('init', () => editor.setContent(this.value.toString()));

		// If we can not find the insert image/media toolbar button
		// Then we need to add an event listener to the editor
		// That will update native browser drag & drop events
		// To update the icon to show you can NOT drop something into the editor
		if (this._configObject.toolbar && !this.#isMediaPickerEnabled()) {
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

	#onChange(value: string) {
		super.value = value;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#isMediaPickerEnabled() {
		return this._configObject.toolbar.includes('umbmediapicker');
	}

	/**
	 * Nothing rendered by default - TinyMCE initialisation creates
	 * a target div and binds the RTE to that element
	 * @returns
	 */
	render() {
		return html``;
	}

	static styles = [
		UUITextStyles,
		css`
			#editor {
				position: relative;
				min-height: 100px;
			}

			.tox-tinymce-aux {
				z-index: 9000;
			}

			.tox-tinymce-inline {
				z-index: 900;
			}

			.tox-tinymce-fullscreen {
				position: absolute;
			}

			/* FIXME: Remove this workaround when https://github.com/tinymce/tinymce/issues/6431 has been fixed */
			.tox .tox-collection__item-label {
				line-height: 1 !important;
			}
		`,
	];
}

export default UmbInputTinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tiny-mce': UmbInputTinyMceElement;
	}
}
