import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	css,
	customElement,
	html,
	property,
	state,
	when,
	LitElement,
	createRef,
	ref,
	type Ref
} from '@umbraco-cms/backoffice/external/lit';
import { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';

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

	private _codeRef: Ref<HTMLElement> = createRef();
	private _rawCode = '';
	private _lastSignature = '';

	get codeLang() {
		const lang = this.language.toLowerCase();

		switch (lang) {
			case 'c#':
			case 'csharp':
				return 'csharp';
			case 'js':
			case 'javascript':
				return 'javascript';
			case 'ts':
			case 'typescript':
				return 'typescript';
			default:
				return lang || 'plaintext';
		}
	}

	async copyCode() {
		if (!this._rawCode) return;

		await navigator.clipboard.writeText(this._rawCode);

		this._copyState = 'success';
		setTimeout(() => (this._copyState = 'idle'), 2000);
	}

	constructor() {
		super();
	}

	render() {
		return html`
			${this.#renderHeader()}
			<pre><uui-scroll-container><code id="code" data-lang=${this.codeLang} ${ref(this._codeRef)}></code></uui-scroll-container></pre>
			<slot @slotchange=${this.#onSlotChanged} style="display:none;"></slot>
		`; // Avoid breaks between elements of <pre></pre>
	}

	async #onSlotChanged(event: Event) {
		const slot = event.target as HTMLSlotElement;

		const nodes = slot.assignedNodes({ flatten: true });

		this._rawCode = nodes.map((n) => n.textContent ?? '').join('');

		await this.updateComplete;
		this.#highlight();
	}

	async #highlight() {
		const el = this._codeRef.value;
		if (!el || !this._rawCode) return;

		const signature = `${this.codeLang}:${this._rawCode}`;

		// Skip if nothing changed
		if (this._lastSignature === signature) {
			return;
		}

		el.textContent = this._rawCode;

		// Apply highlighting
		monaco.editor.colorizeElement(el, {
			mimeType: this.#mapToMime(this.codeLang),
		});

		// Store new state
		this._lastSignature = signature;
	}

	#mapToMime(lang: string) {
		switch (lang) {
			case 'css':
				return 'text/css';
			case 'json':
				return 'application/json';
			case 'javascript':
				return 'text/javascript';
			case 'typescript':
				return 'text/typescript';
			case 'csharp':
				return 'text/x-csharp';
			default:
				return 'text/plain';
		}
	}

	// Re-highlight if language changes
	protected updated(changed: Map<string, unknown>) {
		if (changed.has('language')) {
			this.updateComplete.then(() => this.#highlight());
		}
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
