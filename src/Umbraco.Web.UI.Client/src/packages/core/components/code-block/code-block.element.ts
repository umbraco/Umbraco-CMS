import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { Ref } from '@umbraco-cms/backoffice/external/lit';
import type { UUIScrollContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { createRef, css, customElement, html, property, ref, state, when, LitElement } from '@umbraco-cms/backoffice/external/lit';
//import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
import { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';

//import { jsonDefaults } from 'monaco-editor/esm/vs/language/json/monaco.contribution';
//import { setupMode } from 'monaco-editor/esm/vs/language/json/jsonMode';

//TODO consider adding a highlight prop to the code block, that could spin up/access monaco instance and highlight the code

/**
 *  A simple styled box for showing code-based error messages or blocks od code.
 *  @slot - the default slot where the full message resides
 */
@customElement('umb-code-block')
export class UmbCodeBlockElement extends LitElement {
	@property()
	language = '';

	@property({ type: Boolean })
	copy = false;

	@state()
	private _copyState: 'idle' | 'success' = 'idle';

	private _lang = '';

	private containerRef: Ref<HTMLElement> = createRef();

	get container() {
		if (!this.containerRef?.value) throw new Error('Container element not found');
		return this.containerRef!.value;
	}

	get codeLang() {
		this._lang = this.language.toLowerCase();

		switch (this._lang) {
			case 'c#':
			case 'csharp':
				this._lang = 'csharp';
				break;
			case 'ts':
			case 'typescript':
				monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
					noSemanticValidation: false,
					noSyntaxValidation: false,
				});
				this._lang = 'typescript';
				break;
			case 'js':
			case 'javascript':
				this._lang = 'javascript';
				break;
		}

		return this._lang;
	}

	async copyCode() {
		const text = this.textContent;
		if (text) {
			await navigator.clipboard.writeText(text);
			this._copyState = 'success';
			setTimeout(() => {
				this._copyState = 'idle';
			}, 2000);
		}
	}

	constructor() {
		super();
		//setupMode(jsonDefaults);
	}

	override firstUpdated(): void {
		
	}

	override render() {
		return html`
			${this.#renderHeader()}
			<pre><uui-scroll-container><code id="code" ${ref(this.containerRef)} data-lang=${this.codeLang}><slot @slotchange=${this.#onSlotChanged}></slot></code></uui-scroll-container></pre>
		`; // Avoid breaks between elements of <pre></pre>
	}

	#syntaxHighlight() {
		if (!this.codeLang) return;

		monaco.editor.colorizeElement(this.container, {});
		//monaco.editor.colorizeElement(this.shadowRoot!.getElementById("code")!, {});

		let mimeType = '';

		if (this.codeLang === 'json') {
			mimeType = 'application/json';
		}
		else if (this.codeLang === 'css') {
			mimeType = 'text/css';
		}

		monaco.editor.colorize(this.textContent, this.codeLang, {
			mimeType: mimeType,
		})
		.then(html => {
			this.container.innerHTML = html;
		});
	}

	#onSlotChanged(event: Event) {
		const slot = event.target as HTMLSlotElement;
		const name = slot.name;

		console.log("onSlotChanged")

		this.#syntaxHighlight();
		
		//this.requestUpdate();
	}

	#renderHeader() {
		if (!this.language && !this.copy) return;
		return html`
			<div id="header">
				<span id="lang">${this.language}</span>
				${when(
					this.copy,
					() => html`
						<uui-button color=${this._copyState === 'idle' ? 'default' : 'positive'} @click=${this.copyCode}>
							${when(
								this._copyState === 'idle',
								() => html`<uui-icon name="copy"></uui-icon> <umb-localize key="general_copy">Copy</umb-localize>`,
								() =>
									html`<uui-icon name="check"></uui-icon> <umb-localize key="general_copied">Copied!</umb-localize>`,
							)}
						</uui-button>
					`,
				)}
			</div>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				border: 1px solid var(--uui-color-divider-emphasis);
				border-radius: var(--uui-border-radius);
			}

			uui-scroll-container {
				overflow-y: auto;
				overflow-wrap: anywhere;
			}

			pre {
				font-family: monospace;
				background-color: var(--uui-color-surface-alt);
				color: #303033;
				display: block;
				margin: 0;
				overflow-x: auto;
				padding: 9.5px;
			}

			pre,
			code {
				word-wrap: normal;
				white-space: pre;
				color: var(--uui-color-text);
			}

			#header {
				display: flex;
				justify-content: space-between;
				align-items: center;
				background-color: var(--uui-color-surface-alt);
				border-bottom: 1px solid var(--uui-color-divider-emphasis);
			}

			#lang {
				margin-left: 16px;
				font-weight: bold;
			}

			button {
				font-family: inherit;
				padding: 6px 16px;
				background-color: transparent;
				border: none;
				border-left: 1px solid var(--uui-color-divider-emphasis);
				border-radius: 0;
				color: #000;
				display: flex;
				align-items: center;
				gap: 8px;
			}

			button:hover {
				background-color: var(--uui-color-surface-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-block': UmbCodeBlockElement;
	}
}
