import type { UmbTiptapExtensionApi } from '../../extensions/types.js';
import type { UmbTiptapToolbarValue } from '../types.js';
import { css, customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { Editor, Placeholder, StarterKit, TextStyle } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './tiptap-hover-menu.element.js';
import './tiptap-toolbar.element.js';

const elementName = 'umb-input-tiptap';

@customElement(elementName)
export class UmbInputTiptapElement extends UmbFormControlMixin<string, typeof UmbLitElement, string>(UmbLitElement) {
	readonly #requiredExtensions = [
		StarterKit,
		Placeholder.configure({
			placeholder: ({ node }) => {
				if (node.type.name === 'heading') {
					return this.localize.term('placeholders_rteHeading');
				}

				return this.localize.term('placeholders_rteParagraph');
			},
		}),
		TextStyle,
	];

	@state()
	private readonly _extensions: Array<UmbTiptapExtensionApi> = [];

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
	_toolbar: UmbTiptapToolbarValue = [[[]]];

	protected override async firstUpdated() {
		await Promise.all([await this.#loadExtensions(), await this.#loadEditor()]);
	}

	async #loadExtensions() {
		await new Promise<void>((resolve) => {
			this.observe(umbExtensionsRegistry.byType('tiptapExtension'), async (manifests) => {
				const enabledExtensions = this.configuration?.getValueByAlias<string[]>('extensions') ?? [];
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
		if (dimensions?.width) this.setAttribute('style', `max-width: ${dimensions.width}px;`);
		if (dimensions?.height) element.setAttribute('style', `height: ${dimensions.height}px;`);

		this._toolbar = this.configuration?.getValueByAlias<UmbTiptapToolbarValue>('toolbar') ?? [[[]]];

		const extensions = this._extensions
			.map((ext) => ext.getTiptapExtensions({ configuration: this.configuration }))
			.flat();

		this._editor = new Editor({
			element: element,
			editable: !this.readonly,
			extensions: [...this.#requiredExtensions, ...extensions],
			content: this.#markup,
			onBeforeCreate: ({ editor }) => {
				this._extensions.forEach((ext) => ext.setEditor(editor));
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
				() => html`<div id="loader"><uui-loader></uui-loader></div>`,
				() => html`
					<umb-tiptap-toolbar
						.toolbar=${this._toolbar}
						.editor=${this._editor}
						.configuration=${this.configuration}
						?readonly=${this.readonly}></umb-tiptap-toolbar>
				`,
			)}
			<div id="editor"></div>
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

			.tiptap {
				height: 100%;
				width: 100%;
				outline: none;
				white-space: pre-wrap;
				min-width: 0;
			}

			.tiptap .is-editor-empty:first-child::before {
				color: var(--uui-color-text);
				opacity: 0.55;
				content: attr(data-placeholder);
				float: left;
				height: 0;
				pointer-events: none;
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

				figure {
					> p,
					img {
						pointer-events: none;
						margin: 0;
						padding: 0;
					}

					&.ProseMirror-selectednode {
						outline: 3px solid var(--uui-color-focus);
					}
				}

				img {
					&.ProseMirror-selectednode {
						outline: 3px solid var(--uui-color-focus);
					}
				}

				li {
					> p {
						margin: 0;
						padding: 0;
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
				}

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

					.resize-cursor {
						cursor: ew-resize;
						cursor: col-resize;
					}
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
