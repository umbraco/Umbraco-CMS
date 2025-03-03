import { availableLanguages } from './input-tiny-mce.languages.js';
import { defaultFallbackConfig } from './input-tiny-mce.defaults.js';
import { pastePreProcessHandler } from './input-tiny-mce.handlers.js';
import { uriAttributeSanitizer } from './input-tiny-mce.sanitizer.js';
import type { UmbTinyMcePluginBase } from './tiny-mce-plugin.js';
import { css, customElement, html, property, query } from '@umbraco-cms/backoffice/external/lit';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { getProcessedImageUrl, umbDeepMerge } from '@umbraco-cms/backoffice/utils';
import { renderEditor } from '@umbraco-cms/backoffice/external/tinymce';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbStylesheetDetailRepository, UmbStylesheetRuleManager } from '@umbraco-cms/backoffice/stylesheet';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import type { EditorEvent, Editor, RawEditorOptions } from '@umbraco-cms/backoffice/external/tinymce';
import type { ManifestTinyMcePlugin } from '@umbraco-cms/backoffice/tiny-mce';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * Handles the resize event
 * @param e
 */
async function onResize(
	e: EditorEvent<{
		target: HTMLElement;
		width: number;
		height: number;
		origin: string;
	}>,
) {
	const srcAttr = e.target.getAttribute('src');

	if (!srcAttr) {
		return;
	}

	const path = srcAttr.split('?')[0];
	const resizedPath = await getProcessedImageUrl(path, {
		width: e.width,
		height: e.height,
		mode: ImageCropModeModel.MAX,
	});

	e.target.setAttribute('data-mce-src', resizedPath);
}

@customElement('umb-input-tiny-mce')
export class UmbInputTinyMceElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	#plugins: Array<ClassConstructor<UmbTinyMcePluginBase> | undefined> = [];
	#editorRef?: Editor | null = null;
	readonly #stylesheetRepository = new UmbStylesheetDetailRepository(this);
	readonly #umbStylesheetRuleManager = new UmbStylesheetRuleManager();

	protected override getFormElement() {
		return this._editorElement?.querySelector('iframe') ?? undefined;
	}

	override set value(newValue: FormDataEntryValue | FormData) {
		if (newValue === this.value) return;
		super.value = newValue;
		const newContent = typeof newValue === 'string' ? newValue : '';

		if (this.#editorRef && this.#editorRef.getContent() != newContent) {
			this.#editorRef.setContent(newContent);
		}
	}

	override get value(): FormDataEntryValue | FormData {
		return super.value;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public get readonly() {
		return this.#readonly;
	}
	public set readonly(value) {
		this.#readonly = value;
		const editor = this.getEditor();
		const mode = value ? 'readonly' : 'design';
		editor?.mode.set(mode);
	}
	#readonly = false;

	@query('.editor', true)
	private readonly _editorElement?: HTMLElement;

	getEditor() {
		return this.#editorRef;
	}

	override firstUpdated() {
		this.#loadEditor();
	}

	async #loadEditor() {
		this.observe(umbExtensionsRegistry.byType('tinyMcePlugin'), async (manifests) => {
			this.#plugins.length = 0;
			this.#plugins = await this.#loadPlugins(manifests);

			let config: RawEditorOptions = {};
			manifests.forEach((manifest) => {
				if (manifest.meta?.config) {
					config = umbDeepMerge(manifest.meta.config, config);
				}
			});

			this.#setTinyConfig(config);
		});
	}

	override disconnectedCallback() {
		super.disconnectedCallback();

		this.#editorRef?.destroy();
	}

	/**
	 * Load all custom plugins - need to split loading and instantiating as these
	 * need the editor instance as a ctor argument. If we load them in the editor
	 * setup method, the asynchronous nature means the editor is loaded before
	 * the plugins are ready and so are not associated with the editor.
	 * @param manifests
	 */
	async #loadPlugins(manifests: Array<ManifestTinyMcePlugin>) {
		const promises = [];
		for (const manifest of manifests) {
			if (manifest.js) {
				promises.push(await loadManifestApi(manifest.js));
			}
			if (manifest.api) {
				promises.push(await loadManifestApi(manifest.api));
			}
		}
		return promises;
	}

	async getFormatStyles(stylesheetPaths: Array<string>) {
		if (!stylesheetPaths) return [];
		const formatStyles: any[] = [];

		const promises = stylesheetPaths.map((path) => this.#stylesheetRepository?.requestByUnique(path));
		const stylesheetResponses = await Promise.all(promises);

		stylesheetResponses.forEach(({ data }) => {
			if (!data?.content) return;

			const rulesFromContent = this.#umbStylesheetRuleManager.extractRules(data.content);

			rulesFromContent.forEach((rule) => {
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

				formatStyles.push(r);
			});
		});

		return formatStyles;
	}

	async #setTinyConfig(additionalConfig?: RawEditorOptions) {
		const dimensions = this.configuration?.getValueByAlias<{ width?: number; height?: number }>('dimensions');

		const stylesheetPaths = this.configuration?.getValueByAlias<string[]>('stylesheets') ?? [];
		const styleFormats = await this.getFormatStyles(stylesheetPaths);

		// Map the stylesheets with server url
		const stylesheets =
			stylesheetPaths?.map((stylesheetPath: string) => `/css${stylesheetPath.replace(/\\/g, '/')}`) ?? [];

		stylesheets.push('/umbraco/backoffice/css/rte-content.css');

		// create an object by merging the configuration onto the fallback config
		const configurationOptions: RawEditorOptions = {
			...defaultFallbackConfig,
			height: dimensions?.height ?? defaultFallbackConfig.height,
			width: dimensions?.width ?? defaultFallbackConfig.width,
			content_css: stylesheets.length ? stylesheets : defaultFallbackConfig.content_css,
			style_formats: styleFormats.length ? styleFormats : defaultFallbackConfig.style_formats,
		};

		// no auto resize when a fixed height is set
		if (!configurationOptions.height) {
			if (Array.isArray(configurationOptions.plugins) && configurationOptions.plugins.includes('autoresize')) {
				configurationOptions.plugins.splice(configurationOptions.plugins.indexOf('autoresize'), 1);
			}
		}

		// set the configured toolbar if any, otherwise false
		const toolbar = this.configuration?.getValueByAlias<string[]>('toolbar');
		if (toolbar?.length) {
			configurationOptions.toolbar = toolbar.join(' ');
		} else {
			configurationOptions.toolbar = false;
		}

		// set the configured inline mode
		const mode = this.configuration?.getValueByAlias<string>('mode');
		if (mode?.toLocaleLowerCase() === 'inline') {
			configurationOptions.inline = true;
		}

		// set the maximum image size
		const maxImageSize = this.configuration?.getValueByAlias<number>('maxImageSize');
		if (maxImageSize) {
			configurationOptions.maxImageSize = maxImageSize;
		}

		// set the default values that will not be modified via configuration
		let config: RawEditorOptions = {
			autoresize_bottom_margin: 10,
			body_class: 'umb-rte',
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
			language: this.#getLanguage(),
			promotion: false,
			convert_unsafe_embeds: true, // [JOV] Workaround for CVE-2024-29881
			readonly: this.#readonly,

			// Extend with configuration options
			...configurationOptions,
		};

		// Extend with additional configuration options
		if (additionalConfig) {
			config = umbDeepMerge(additionalConfig, config);
		}

		this.#editorRef?.destroy();

		const editors = await renderEditor(config).catch((error) => {
			console.error('Failed to render TinyMCE', error);
			return [];
		});
		this.#editorRef = editors.pop();
	}

	/**
	 * Gets the language to use for TinyMCE
	 */
	#getLanguage() {
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

		return languageMatch;
	}

	#editorSetup(editor: Editor) {
		editor.suffix = '.min';

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
			onResize(e);
			this.#onChange(editor.getContent());
		});

		editor.on('SetContent', () => {
			/**
			 * Prevent injecting arbitrary JavaScript execution in on-attributes.
			 *
			 */
			const allNodes = Array.from(editor.dom.doc.getElementsByTagName('*'));
			allNodes.forEach((node) => {
				for (const attr of node.attributes) {
					if (attr.name.startsWith('on')) {
						node.removeAttribute(attr.name);
					}
				}
			});
		});

		// instantiate plugins to ensure they are available before setting up the editor.
		// Plugins require a reference to the current editor as a param, so can not
		// be instantiated until we have an editor
		for (const plugin of this.#plugins) {
			if (plugin) {
				// [v15]: This might be improved by changing to `createExtensionApi` and avoiding the `#loadPlugins` method altogether, but that would require a breaking change
				// because that function sends the UmbControllerHost as the first argument, which is not the case here.
				new plugin({ host: this, editor });
			}
		}
	}

	#onInit(editor: Editor) {
		//enable browser based spell checking
		editor.getBody().setAttribute('spellcheck', 'true');
		uriAttributeSanitizer(editor);
		editor.setContent(typeof this.value === 'string' ? this.value : '');
	}

	#onChange(value: string) {
		if (this.value === value) return;
		this.value = value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	/**
	 * Nothing rendered by default - TinyMCE initialization creates
	 * a target div and binds the RTE to that element
	 */
	override render() {
		return html`<div class="editor"></div>`;
	}

	static override readonly styles = [
		css`
			.tox-tinymce {
				position: relative;
				min-height: 100px;
				border-radius: 0;
				border: var(--uui-input-border-width, 1px) solid var(--uui-input-border-color, var(--uui-color-border, #d8d7d9));
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
