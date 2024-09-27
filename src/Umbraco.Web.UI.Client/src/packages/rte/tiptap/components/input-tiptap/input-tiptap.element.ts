import type { UmbTiptapExtensionApi } from '../../extensions/types.js';
import { css, customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	Document,
	Dropcursor,
	Editor,
	Gapcursor,
	HardBreak,
	History,
	Paragraph,
	Text,
} from '@umbraco-cms/backoffice/external/tiptap';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './tiptap-fixed-menu.element.js';
import './tiptap-hover-menu.element.js';

const elementName = 'umb-input-tiptap';

@customElement(elementName)
export class UmbInputTiptapElement extends UmbFormControlMixin<string, typeof UmbLitElement, string>(UmbLitElement) {
	readonly #requiredExtensions = [Document, Dropcursor, Gapcursor, HardBreak, History, Paragraph, Text];

	@state()
	private _extensions: Array<UmbTiptapExtensionApi> = [];

	@property({ type: String })
	override set value(value: string) {
		this.#markup = value;

		// Try to set the value to the editor if it is ready.
		if (this._editor) {
			this._editor.commands.setContent(value);
		}
	}
	override get value() {
		return this.#markup;
	}

	#markup = '';

	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _editor!: Editor;

	@state()
	private _toolbarConfig: Array<{
		alias: string;
		position?: [number, number, number];
	}> = [];

	protected override async firstUpdated() {
		// TODO: we need some types here
		this._toolbarConfig = (this.configuration?.getValueByAlias<string>('toolbar') as any) ?? [];
		await Promise.all([await this.#loadExtensions(), await this.#loadEditor()]);
	}

	async #loadExtensions() {
		await new Promise<void>((resolve) => {
			this.observe(umbExtensionsRegistry.byType('tiptapExtension'), async (manifests) => {
				manifests = manifests.filter((ext) => {
					return !!this._toolbarConfig.find((x) => x.alias === ext.alias);
				});

				for (const manifest of manifests) {
					if (manifest.api) {
						const extension = await loadManifestApi(manifest.api);
						if (extension) {
							this._extensions.push(new extension(this));
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

		const maxWidth = this.configuration?.getValueByAlias<number>('maxWidth');
		const maxHeight = this.configuration?.getValueByAlias<number>('maxHeight');
		if (maxWidth) this.setAttribute('style', `max-width: ${maxWidth}px;`);
		if (maxHeight) element.setAttribute('style', `max-height: ${maxHeight}px;`);

		const extensions = this._extensions
			.map((ext) => ext.getTiptapExtensions({ configuration: this.configuration }))
			.flat();

		this._editor = new Editor({
			element: element,
			editable: !this.readonly,
			extensions: [...this.#requiredExtensions, ...extensions],
			content: this.#markup,
			onBeforeCreate: ({ editor }) => {
				this._extensions.forEach((ext) => {
					console.log('🚀 ~ this._extensions.forEach ~ ext:', ext);
					ext.setEditor(editor);
				});
			},
			onUpdate: ({ editor }) => {
				this.#markup = editor.getHTML();
				this.dispatchEvent(new UmbChangeEvent());
			},
		});
	}

	override render() {
		return html`
			${when(
				!this._editor && !this._extensions?.length,
				() => html`<uui-loader></uui-loader>`,
				() => html`
					<umb-tiptap-fixed-menu
						.editor=${this._editor}
						.toolbarConfig=${this._toolbarConfig}
						?readonly=${this.readonly}></umb-tiptap-fixed-menu>
				`,
			)}
			<div id="editor"></div>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
			}
			:host([readonly]) {
				pointer-events: none;

				#editor {
					background-color: var(--uui-color-surface-alt);
				}
			}

			.tiptap {
				height: 100%;
				width: 100%;
				outline: none;
				white-space: pre-wrap;
				min-width: 0;
			}

			#editor {
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
				min-height: 400px;
				display: grid; /* Don't ask me why this is needed, but it is. */

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
				h3 {
					margin-top: 0;
					margin-bottom: 0.5em;
				}

				figure {
					> p,
					img {
						pointer-events: none;
						margin: 0;
						padding: 0;
					}

					&.ProseMirror-selectednode {
						outline: 3px solid #b4d7ff;
					}
				}

				img {
					&.ProseMirror-selectednode {
						outline: 3px solid #b4d7ff;
					}
				}

				.umb-embed-holder {
					display: inline-block;
					position: relative;
				}

				.umb-embed-holder > * {
					user-select: none;
					pointer-events: none;
				}

				.umb-embed-holder.ProseMirror-selectednode {
					outline: 2px solid var(--uui-palette-spanish-pink-light);
				}

				.umb-embed-holder::before {
					z-index: 1000;
					width: 100%;
					height: 100%;
					position: absolute;
					content: ' ';
				}

				.umb-embed-holder.ProseMirror-selectednode::before {
					background: rgba(0, 0, 0, 0.025);
				/* Table-specific styling */
				.tableWrapper {
					margin: 1.5rem 0;
					overflow-x: auto;

					table {
						border-collapse: collapse;
						margin: 0;
						overflow: hidden;
						table-layout: fixed;
						width: 100%;

						td,
						th {
							border: 1px solid var(--uui-color-border);
							box-sizing: border-box;
							min-width: 1em;
							padding: 6px 8px;
							position: relative;
							vertical-align: top;

							> * {
								margin-bottom: 0;
							}
						}

						th {
							background-color: var(--uui-color-background);
							font-weight: bold;
							text-align: left;
						}

						.selectedCell:after {
							background: var(--uui-color-surface-emphasis);
							content: '';
							left: 0;
							right: 0;
							top: 0;
							bottom: 0;
							pointer-events: none;
							position: absolute;
							z-index: 2;
						}

						.column-resize-handle {
							background-color: var(--uui-color-default);
							bottom: -2px;
							pointer-events: none;
							position: absolute;
							right: -2px;
							top: 0;
							width: 3px;
						}
					}
				}

				.resize-cursor {
					cursor: ew-resize;
					cursor: col-resize;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbInputTiptapElement;
	}
}
