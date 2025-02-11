import type { UmbTiptapExtensionApi } from '../../extensions/types.js';
import type { UmbTiptapToolbarValue } from '../types.js';
import { css, customElement, html, property, state, unsafeCSS, when } from '@umbraco-cms/backoffice/external/lit';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import type { Extensions } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './tiptap-hover-menu.element.js';
import './tiptap-toolbar.element.js';

const TIPTAP_CORE_EXTENSION_ALIAS = 'Umb.Tiptap.RichTextEssentials';

@customElement('umb-input-tiptap')
export class UmbInputTiptapElement extends UmbFormControlMixin<string, typeof UmbLitElement, string>(UmbLitElement) {
	@property({ type: String })
	override set value(value: string) {
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
	_toolbar: UmbTiptapToolbarValue = [[[]]];

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
		await new Promise<void>((resolve) => {
			this.observe(umbExtensionsRegistry.byType('tiptapExtension'), async (manifests) => {
				let enabledExtensions = this.configuration?.getValueByAlias<string[]>('extensions') ?? [];

				// Ensures that the "Rich Text Essentials" extension is always enabled. [LK]
				if (!enabledExtensions.includes(TIPTAP_CORE_EXTENSION_ALIAS)) {
					enabledExtensions = [TIPTAP_CORE_EXTENSION_ALIAS, ...enabledExtensions];
				}

				for (const manifest of manifests) {
					if (manifest.api) {
						const extension = await loadManifestApi(manifest.api);
						if (extension) {
							// Check if the extension is enabled
							if (enabledExtensions.includes(manifest.alias)) {
								this._extensions.push(new extension(this));
							}
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

		const dimensions = this.configuration?.getValueByAlias<{ width?: number; height?: number }>('dimensions');
		if (dimensions?.width) {
			this.setAttribute('style', `max-width: ${dimensions.width}px;`);
		}
		if (dimensions?.height) {
			element.setAttribute('style', `height: ${dimensions.height}px;`);
		}

		this._toolbar = this.configuration?.getValueByAlias<UmbTiptapToolbarValue>('toolbar') ?? [[[]]];

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
			extensions: tiptapExtensions,
			content: this.#value,
			onBeforeCreate: ({ editor }) => {
				this._extensions.forEach((ext) => ext.setEditor(editor));
			},
			onUpdate: ({ editor }) => {
				this.#value = editor.getHTML();
				this.dispatchEvent(new UmbChangeEvent());
			},
		});
	}

	override render() {
		return html`
			${when(
				!this._editor && !this._extensions?.length,
				() => html`<div id="loader"><uui-loader></uui-loader></div>`,
				() => html`
					${this.#renderStyles()}
					<umb-tiptap-toolbar
						.toolbar=${this._toolbar}
						.editor=${this._editor}
						.configuration=${this.configuration}
						?readonly=${this.readonly}></umb-tiptap-toolbar>
				`,
			)}
			<div id="editor"></div>
			<div id="umbTableColumnMenu" style="visibility: collapse;--uui-menu-item-flat-structure: 1;">
				<uui-menu-item label="Add column before" @click-label=${() => this._editor?.chain().focus().addColumnBefore().run()}><umb-icon slot="icon" name="icon-table"></umb-icon></uui-menu-item>
				<uui-menu-item label="Add column after" @click-label=${() => this._editor?.chain().focus().addColumnAfter().run()}><umb-icon slot="icon" name="icon-table"></umb-icon></uui-menu-item>
				<uui-menu-item label="Delete column" @click-label=${() => this._editor?.chain().focus().deleteColumn().run()}><umb-icon slot="icon" name="icon-table"></umb-icon></uui-menu-item>
			</div>
			<div id="umbTableRowMenu" style="visibility: collapse;--uui-menu-item-flat-structure: 1;">
				<uui-menu-item label="Add row before" @click-label=${() => this._editor?.chain().focus().addRowBefore().run()}><umb-icon slot="icon" name="icon-table"></umb-icon></uui-menu-item>
				<uui-menu-item label="Add row after" @click-label=${() => this._editor?.chain().focus().addRowAfter().run()}><umb-icon slot="icon" name="icon-table"></umb-icon></uui-menu-item>
				<uui-menu-item label="Delete row" @click-label=${() => this._editor?.chain().focus().deleteRow().run()}><umb-icon slot="icon" name="icon-table"></umb-icon></uui-menu-item>
			</div>
		`;
	}

	#renderStyles() {
		if (!this._styles?.length) return;
		return html`
			<style>
				${this._styles.map((style) => unsafeCSS(style))}
			</style>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
				position: relative;
				z-index: 0;
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

			#editor {
				/* Required as overflow is set to auto, so that the scrollbars don't appear. */
				display: flex;
				overflow: auto;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-border);
				padding: 1rem;
				border-top-left-radius: 0;
				border-top-right-radius: 0;
				border-top: 0;
				box-sizing: border-box;
				height: 100%;
				width: 100%;

				.tiptap {
					height: 100%;
					width: 100%;
					outline: none;
					white-space: pre-wrap;
					min-width: 0;

					.is-editor-empty:first-child::before {
						color: var(--uui-color-text);
						opacity: 0.55;
						content: attr(data-placeholder);
						float: left;
						height: 0;
						pointer-events: none;
					}
				}

				/* The following styles are required for the "StarterKit" extension. */
				pre {
					background-color: var(--uui-color-surface-alt);
					padding: var(--uui-size-space-2) var(--uui-size-space-4);
					border-radius: calc(var(--uui-border-radius) * 2);
					overflow-x: auto;
				}

				code:not(pre > code) {
					background-color: var(--uui-color-surface-alt);
					padding: var(--uui-size-space-1) var(--uui-size-space-2);
					border-radius: calc(var(--uui-border-radius) * 2);
				}

				code {
					font-family: 'Roboto Mono', monospace;
					background: none;
					color: inherit;
					font-size: 0.8rem;
					padding: 0;
				}

				h1,
				h2,
				h3,
				h4,
				h5,
				h6 {
					margin-top: 0;
					margin-bottom: 0.5em;
				}

				li {
					> p {
						margin: 0;
						padding: 0;
					}
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tiptap': UmbInputTiptapElement;
	}
}
