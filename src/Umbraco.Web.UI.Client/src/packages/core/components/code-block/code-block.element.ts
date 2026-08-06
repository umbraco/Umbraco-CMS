import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

//TODO consider adding a highlight prop to the code block, that could spin up/access monaco instance and highlight the code

/**
 *  A simple styled box for showing code-based error messages or blocks od code.
 *  @slot - the default slot where the full message resides
 */
@customElement('umb-code-block')
export class UmbCodeBlockElement extends UmbLitElement {
	@property()
	language = '';

	@property({ type: Boolean })
	copy = false;

	@state()
	private _copyState: 'idle' | 'success' = 'idle';

	#notificationContext?: UmbNotificationContext;

	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => (this.#notificationContext = instance));
	}

	async copyCode() {
		const text = this.textContent;
		if (!text) return;

		try {
			// navigator.clipboard only exists in secure contexts (https or localhost) and the
			// write can also be denied by permissions policy, so failures must be surfaced.
			await navigator.clipboard.writeText(text);
			this._copyState = 'success';
			setTimeout(() => {
				this._copyState = 'idle';
			}, 2000);
		} catch {
			this.#notificationContext?.peek('danger', {
				data: { message: this.localize.term('speechBubbles_cannotCopyToClipboard') },
			});
		}
	}

	override render() {
		return html`
			${this.#renderHeader()}
			<pre><uui-scroll-container><code><slot></slot></code></uui-scroll-container></pre>
		`; // Avoid breaks between elements of <pre></pre>
	}

	#renderHeader() {
		if (!this.language && !this.copy) return;
		return html`
			<div id="header">
				<span id="lang">${this.language}</span>
				${when(
					this.copy,
					() => html`
						<uui-button
							.label=${this._copyState === 'idle'
								? this.localize.term('general_copy')
								: this.localize.term('general_copied')}
							@click=${this.copyCode}
							compact>
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
				background-color: var(--uui-color-surface);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
			}

			uui-scroll-container {
				overflow-y: auto;
				overflow-wrap: anywhere;
			}

			pre {
				font-family: monospace;
				color: #303033;
				display: block;
				margin: 0;
				overflow-x: auto;
				padding: var(--uui-size-space-3);
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
				border-bottom: 1px solid var(--uui-color-border);
			}

			#header uui-button {
				margin-right: var(--uui-size-space-1);
			}

			#lang {
				margin-left: var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-block': UmbCodeBlockElement;
	}
}
