import { defaultFallbackConfig } from './input-tiny-mce.defaults.js';
import { pastePreProcessHandler } from './input-tiny-mce.handlers.js';
import { availableLanguages } from './input-tiny-mce.languages.js';
import { uriAttributeSanitizer } from './input-tiny-mce.sanitizer.js';
import { umbMeta } from '@umbraco-cms/backoffice/meta';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { type Editor, type RawEditorOptions, renderEditor } from '@umbraco-cms/backoffice/external/tinymce';
import { TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/components';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { ManifestTinyMcePlugin, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	PropertyValueMap,
	css,
	customElement,
	html,
	property,
	query,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbMediaHelper } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { UmbStylesheetDetailRepository } from '@umbraco-cms/backoffice/stylesheet';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-tiny-mce')
export class UmbInputTinyMceElement extends FormControlMixin(UmbLitElement) {
	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	@state()
	private _tinyConfig: RawEditorOptions = {};

	#mediaHelper = new UmbMediaHelper();
	#plugins: Array<new (args: TinyMcePluginArguments) => UmbTinyMcePluginBase> = [];
	#editorRef?: Editor | null = null;
	#stylesheetRepository?: UmbStylesheetDetailRepository;
	#serverUrl?: string;

	protected getFormElement() {
		return this._editorElement?.querySelector('iframe') ?? undefined;
	}

	@query('#editor', true)
	private _editorElement?: HTMLElement;

	constructor() {
		super();

		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		});

		this.#stylesheetRepository = new UmbStylesheetDetailRepository(this);
	}

	protected async firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): Promise<void> {
		super.firstUpdated(_changedProperties);
		await this.#loadPlugins();
		await this.#setTinyConfig();
	}

	disconnectedCallback() {
		super.disconnectedCallback();

		if (this.#editorRef) {
			// TODO: Test if there is any problems with destroying the RTE here, but not initializing on connectedCallback. (firstUpdated is only called first time the element is rendered, not when it is reconnected)
			this.#editorRef.destroy();
		}
	}

	/**
	 * Load all custom plugins - need to split loading and instantiating as these
	 * need the editor instance as a ctor argument. If we load them in the editor
	 * setup method, the asynchronous nature means the editor is loaded before
	 * the plugins are ready and so are not associated with the editor.
	 */
	async #loadPlugins() {
		const observable = umbExtensionsRegistry?.extensionsOfType('tinyMcePlugin');
		const manifests = (await firstValueFrom(observable)) as ManifestTinyMcePlugin[];

		for (const manifest of manifests) {
			const plugin = manifest.js
				? await loadManifestApi(manifest.js)
				: manifest.api
				? await loadManifestApi(manifest.api)
				: undefined;
			if (plugin) {
				this.#plugins.push(plugin);
			}
		}
	}

	async getFormatStyles(stylesheetPath: Array<string>) {
		const rules: any[] = [];

		stylesheetPath.forEach((path) => {
			this.#stylesheetRepository?.getStylesheetRules(path).then(({ data }) => {
				data?.rules?.forEach((rule) => {
					const r: {
						title?: string;
						inline?: string;
						classes?: string;
						attributes?: Record<string, string>;
						block?: string;
					} = {
						title: rule.name,
					};

					if (!rule.selector) return;

					if (rule.selector.startsWith('.')) {
						r.inline = 'span';
						r.classes = rule.selector.substring(1);
					} else if (rule.selector.startsWith('#')) {
						r.inline = 'span';
						r.attributes = { id: rule.selector.substring(1) };
					} else if (rule.selector.includes('.')) {
						const [block, ...classes] = rule.selector.split('.');
						r.block = block;
						r.classes = classes.join(' ').replace(/\./g, ' ');
					} else if (rule.selector.includes('#')) {
						const [block, id] = rule.selector.split('#');
						r.block = block;
						r.classes = id;
					} else {
						r.block = rule.selector;
					}

					rules.push(r);
				});
			});
		});

		return rules;
	}

	async #setTinyConfig() {
		const dimensions = this.configuration?.getValueByAlias<{ width?: number; height?: number }>('dimensions');

		// Map the stylesheets with server url
		const stylesheets =
			this.configuration
				?.getValueByAlias<string[]>('stylesheets')
				?.map((stylesheetPath: string) => `${this.#serverUrl}/css/${stylesheetPath.replace(/\\/g, '/')}`) ?? [];
		const styleFormats = await this.getFormatStyles(stylesheets);

		// create an object by merging the configuration onto the fallback config
		const configurationOptions: RawEditorOptions = {
			...defaultFallbackConfig,
			height: dimensions?.height,
			width: dimensions?.width,
			content_css: stylesheets,
			style_formats: styleFormats,
		};

		// no auto resize when a fixed height is set
		if (!configurationOptions.height) {
			if (Array.isArray(configurationOptions.plugins) && configurationOptions.plugins.includes('autoresize')) {
				configurationOptions.plugins.splice(configurationOptions.plugins.indexOf('autoresize'), 1);
			}
		}

		// set the configured toolbar if any
		const toolbar = this.configuration?.getValueByAlias<string[]>('toolbar');
		if (toolbar) {
			configurationOptions.toolbar = toolbar.join(' ');
		}

		// set the configured inline mode
		const mode = this.configuration?.getValueByAlias<string>('mode');
		if (mode?.toLocaleLowerCase() === 'inline') {
			configurationOptions.inline = true;
		}

		// set the maximum image size
		const maxImageSize = this.configuration?.getValueByAlias<number>('maxImageSize');
		if (maxImageSize !== undefined) {
			configurationOptions.maxImageSize = maxImageSize;
		}

		// set the default values that will not be modified via configuration
		this._tinyConfig = {
			autoresize_bottom_margin: 10,
			body_class: 'umb-rte',
			//see https://www.tiny.cloud/docs/tinymce/6/editor-important-options/#cache_suffix
			cache_suffix: `?umb__rnd=${umbMeta.clientVersion}`,
			contextMenu: false,
			inline_boundaries_selector: 'a[href],code,.mce-annotation,.umb-embed-holder,.umb-macro-holder',
			menubar: false,
			paste_remove_styles_if_webkit: true,
			paste_preprocess: pastePreProcessHandler,
			relative_urls: false,
			resize: false,
			statusbar: false,
			setup: (editor) => this.#editorSetup(editor),
			target: this._editorElement,
			paste_data_images: false,

			// Extend with configuration options
			...configurationOptions,
		};

		this.#setLanguage();

		if (this.#editorRef) {
			this.#editorRef.destroy();
		}

		const editors = await renderEditor(this._tinyConfig);
		this.#editorRef = editors.pop();
	}

	/**
	 * Sets the language to use for TinyMCE */
	#setLanguage() {
		const localeId = this.localize.lang();
		//try matching the language using full locale format
		let languageMatch = availableLanguages.find((x) => localeId?.localeCompare(x) === 0);

		//if no matches, try matching using only the language
		if (!languageMatch) {
			const localeParts = localeId?.split('_');
			if (localeParts) {
				languageMatch = availableLanguages.find((x) => x === localeParts[0]);
			}
		}

		// only set if language exists, will fall back to tiny default
		if (languageMatch) {
			this._tinyConfig.language = languageMatch;
		}
	}

	#editorSetup(editor: Editor) {
		editor.suffix = '.min';

		// instantiate plugins - these are already loaded in this.#loadPlugins
		// to ensure they are available before setting up the editor.
		// Plugins require a reference to the current editor as a param, so can not
		// be instantiated until we have an editor
		for (const plugin of this.#plugins) {
			new plugin({ host: this, editor });
		}

		// define keyboard shortcuts
		editor.addShortcut('Ctrl+S', '', () =>
			this.dispatchEvent(new CustomEvent('rte.shortcut.save', { composed: true, bubbles: true })),
		);

		editor.addShortcut('Ctrl+P', '', () =>
			this.dispatchEvent(new CustomEvent('rte.shortcut.saveAndPublish', { composed: true, bubbles: true })),
		);

		// bind editor events
		editor.on('init', () => this.#onInit(editor));
		editor.on('Change', () => this.#onChange(editor.getContent()));
		editor.on('Dirty', () => this.#onChange(editor.getContent()));
		editor.on('Keyup', () => this.#onChange(editor.getContent()));

		editor.on('focus', () => this.dispatchEvent(new CustomEvent('umb-rte-focus', { composed: true, bubbles: true })));

		editor.on('blur', () => {
			this.#onChange(editor.getContent());
			this.dispatchEvent(new CustomEvent('umb-rte-blur', { composed: true, bubbles: true }));
		});

		editor.on('ObjectResized', (e) => {
			this.#mediaHelper.onResize(e);
			this.#onChange(editor.getContent());
		});

		editor.on('SetContent', (e) => {
			/**
			 * Prevent injecting arbitrary JavaScript execution in on-attributes.
			 *
			 * TODO: This used to be toggleable through server variables with window.Umbraco?.Sys.ServerVariables.umbracoSettings.sanitizeTinyMce
			 */
			const allNodes = Array.from(editor.dom.doc.getElementsByTagName('*'));
			allNodes.forEach((node) => {
				for (let i = 0; i < node.attributes.length; i++) {
					if (node.attributes[i].name.startsWith('on')) {
						node.removeAttribute(node.attributes[i].name);
					}
				}
			});
		});

		editor.on('init', () => editor.setContent(this.value?.toString() ?? ''));
	}

	#onInit(editor: Editor) {
		//enable browser based spell checking
		editor.getBody().setAttribute('spellcheck', 'true');
		uriAttributeSanitizer(editor);
	}

	#onChange(value: string) {
		this.value = value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	/**
	 * Nothing rendered by default - TinyMCE initialisation creates
	 * a target div and binds the RTE to that element
	 */
	render() {
		return html`<div id="editor"></div>`;
	}

	static styles = [
		css`
			#editor {
				position: relative;
				min-height: 100px;
			}

			.tox-tinymce {
				border-radius: 0;
				border: var(--uui-input-border-width, 1px) solid var(--uui-input-border-color, var(--uui-color-border, #d8d7d9));
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
