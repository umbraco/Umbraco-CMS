import { UmbTiptapRteContext } from '../../contexts/tiptap-rte.context.js';
import type { UmbTiptapExtensionApi } from '../../extensions/types.js';
import type { UmbTiptapStatusbarValue, UmbTiptapToolbarValue } from '../types.js';
import {
	css,
	customElement,
	html,
	property,
	repeat,
	state,
	unsafeCSS,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import type { Extensions } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import '../toolbar/tiptap-toolbar.element.js';
import '../statusbar/tiptap-statusbar.element.js';

const TIPTAP_CORE_EXTENSION_ALIAS = 'Umb.Tiptap.RichTextEssentials';

/**
 * The root path for the stylesheets on the server.
 * This is used to load the stylesheets from the server as a workaround until the server supports virtual paths.
 */
const STYLESHEET_ROOT_PATH = '/css';

@customElement('umb-input-tiptap')
export class UmbInputTiptapElement extends UmbFormControlMixin<string, typeof UmbLitElement, string>(UmbLitElement) {
	#context = new UmbTiptapRteContext(this);

	#stylesheets = new Set(['/umbraco/backoffice/css/rte-content.css']);

	@property({ type: String })
	override set value(value: string) {
		if (value === this.#value) return;
		this.#value = value;

		// Try to set the value to the editor if it is ready.
		if (this._editor) {
			this._editor.commands.setContent(value);
		}
	}
	override get value() {
		return this.#value;
	}
	#value = '';

	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	@property()
	label?: string;

	/**
	 * Sets the input to required, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	required?: boolean;

	@property({ type: String })
	requiredMessage?: string;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _editor?: Editor;

	@state()
	private readonly _extensions: Array<UmbTiptapExtensionApi> = [];

	@state()
	private _styles: Array<CSSResultGroup> = [];

	@state()
	private _toolbar: UmbTiptapToolbarValue = [[[]]];

	@state()
	private _statusbar: UmbTiptapStatusbarValue = [[], []];

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage ?? 'Value is required',
			() => !!this.required && this.isEmpty(),
		);
	}

	protected override async firstUpdated() {
		await Promise.all([await this.#loadExtensions(), await this.#loadEditor()]);
	}

	/**
	 * Checks if the editor is empty.
	 * @returns {boolean} returns true if the editor contains no markup
	 */
	public isEmpty(): boolean {
		return this._editor?.isEmpty ?? false;
	}

	async #loadExtensions() {
		const enabledExtensions = this.configuration?.getValueByAlias<string[]>('extensions') ?? [];

		// Ensures that the "Rich Text Essentials" extension is always enabled. [LK]
		if (!enabledExtensions.includes(TIPTAP_CORE_EXTENSION_ALIAS)) {
			const { default: api } = await import('../../extensions/core/rich-text-essentials.tiptap-api.js');
			this._extensions.push(new api(this));
		}

		await new Promise<void>((resolve) => {
			this.observe(umbExtensionsRegistry.byTypeAndAliases('tiptapExtension', enabledExtensions), async (manifests) => {
				for (const manifest of manifests) {
					if (manifest.api) {
						const extension = await loadManifestApi(manifest.api);
						if (extension) {
							const ext = new extension(this);
							ext.manifest = manifest;
							this._extensions.push(ext);
						}
					}
				}
				resolve();
			});
		});
	}

	async #loadEditor() {
		const element = this.shadowRoot?.querySelector('#editor');
		if (!element) return;

		const stylesheets = this.configuration?.getValueByAlias<Array<string>>('stylesheets');
		if (stylesheets?.length) {
			stylesheets.forEach((stylesheet) => {
				const linkHref =
					stylesheet.startsWith('http') || stylesheet.startsWith(STYLESHEET_ROOT_PATH)
						? stylesheet
						: `${STYLESHEET_ROOT_PATH}${stylesheet}`;
				this.#stylesheets.add(linkHref);
			});
		}

		this._toolbar = this.configuration?.getValueByAlias<UmbTiptapToolbarValue>('toolbar') ?? [[[]]];
		this._statusbar = this.configuration?.getValueByAlias<UmbTiptapStatusbarValue>('statusbar') ?? [];

		const tiptapExtensions: Extensions = [];

		this._extensions.forEach((ext) => {
			const tiptapExt = ext.getTiptapExtensions({ configuration: this.configuration });
			if (tiptapExt?.length) {
				tiptapExtensions.push(...tiptapExt);
			}

			const styles = ext.getStyles();
			if (styles) {
				this._styles.push(styles);
			}
		});

		this._editor = new Editor({
			element: element,
			editable: !this.readonly,
			editorProps: { attributes: { 'aria-label': this.label ?? 'Rich Text Editor' } },
			extensions: tiptapExtensions,
			content: this.#value,
			injectCSS: false, // Prevents injecting CSS into `window.document`, as it never applies to the shadow DOM. [LK]
			//enableContentCheck: true,
			onBeforeCreate: ({ editor }) => {
				this._extensions.forEach((ext) => ext.setEditor(editor));
			},
			onContentError: ({ error }) => {
				console.error('contentError', [error.message, error.cause]);
			},
			onUpdate: ({ editor }) => {
				this.#value = editor.getHTML();
				this._runValidators();
				this.dispatchEvent(new UmbChangeEvent());
			},
		});

		this.#context.setEditor(this._editor);
	}

	override render() {
		const loading = !this._editor && !this._extensions?.length;
		return html`
			${when(loading, () => html`<div id="loader"><uui-loader></uui-loader></div>`)}
			${when(!loading, () => html`${this.#renderStyles()}${this.#renderToolbar()}`)}
			<div id="editor" data-mark="input:tiptap-rte" ?data-loaded=${!loading}></div>
			${when(!loading, () => this.#renderStatusbar())}
		`;
	}

	#renderStyles() {
		if (!this._styles?.length) return;
		return html`
			${repeat(
				this.#stylesheets,
				(stylesheet) => stylesheet,
				(stylesheet) => html`<link rel="stylesheet" href=${stylesheet} />`,
			)}
			<style>
				${this._styles.map((style) => unsafeCSS(style))}
			</style>
		`;
	}

	#renderToolbar() {
		if (!this._toolbar.flat(2).length) return;
		return html`
			<umb-tiptap-toolbar
				data-mark="tiptap-toolbar"
				.toolbar=${this._toolbar}
				.editor=${this._editor}
				.configuration=${this.configuration}
				?readonly=${this.readonly}>
			</umb-tiptap-toolbar>
		`;
	}

	#renderStatusbar() {
		if (!this._statusbar.flat().length) return;
		return html`
			<umb-tiptap-statusbar
				data-mark="tiptap-statusbar"
				.statusbar=${this._statusbar}
				.editor=${this._editor}
				.configuration=${this.configuration}
				?readonly=${this.readonly}>
			</umb-tiptap-statusbar>
		`;
	}

	override destroy(): void {
		this._editor?.destroy();
		this._editor = undefined;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
				position: relative;
				z-index: 0;

				width: var(--umb-rte-width, unset);
				min-width: var(--umb-rte-min-width, unset);
				max-width: var(--umb-rte-max-width, 100%);
			}

			:host([readonly]) {
				pointer-events: none;

				#editor {
					background-color: var(--uui-color-surface-alt);
				}
			}

			#loader {
				display: flex;
				align-items: center;
				justify-content: center;
			}

			:host(:invalid:not([pristine])),
			/* polyfill support */
			:host(:not([pristine])[internals-invalid]) {
				--umb-tiptap-edge-border-color: var(--uui-color-invalid);
				#editor {
					border-color: var(--uui-color-invalid);
				}
			}

			#editor {
				display: flex;
				overflow: auto;
				border-radius: var(--uui-border-radius);
				border: 1px solid transparent;
				padding: 1rem;
				box-sizing: border-box;

				height: var(--umb-rte-height, 100%);
				min-height: var(--umb-rte-min-height, 100%);
				max-height: var(--umb-rte-max-height, 100%);

				width: 100%;
				max-width: 100%;

				&[data-loaded] {
					border-color: var(--umb-tiptap-edge-border-color, var(--uui-color-border));
				}

				> .tiptap {
					height: 100%;
					width: 100%;
					outline: none;
					white-space: wrap;
					min-width: 0;

					p:first-of-type {
						margin-top: 0;
					}

					.is-editor-empty:first-child::before {
						color: var(--uui-color-text);
						opacity: 0.55;
						content: attr(data-placeholder);
						float: left;
						height: 0;
						pointer-events: none;
					}
				}

				&:has(+ umb-tiptap-statusbar) {
					border-bottom-left-radius: 0;
					border-bottom-right-radius: 0;
				}
			}

			umb-tiptap-toolbar + #editor {
				border-top: 0;
				border-top-left-radius: 0;
				border-top-right-radius: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tiptap': UmbInputTiptapElement;
	}
}
