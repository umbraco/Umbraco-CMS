import { UMB_BLOCK_LIST_ENTRY_CONTEXT } from '../../index.js';
import type { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../../block/index.js';
import { UMB_BLOCK_WORKSPACE_ALIAS } from '../../../block/index.js';
import { UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import '../../../block/workspace/views/edit/block-workspace-view-edit-content-no-router.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

/**
 * @element umb-inline-list-block
 */
@customElement('umb-inline-list-block')
export class UmbInlineListBlockElement extends UmbLitElement {
	#blockContext?: typeof UMB_BLOCK_LIST_ENTRY_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#contentKey?: string;

	@property({ type: String, reflect: false })
	label?: string;

	@property({ type: String, reflect: false })
	icon?: string;

	@state()
	_isOpen = false;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_LIST_ENTRY_CONTEXT, (blockContext) => {
			this.#blockContext = blockContext;
			this.observe(
				this.#blockContext.unique,
				(contentKey) => {
					this.#contentKey = contentKey;
					this.#load();
				},
				'observeContentKey',
			);
		});
		this.observe(umbExtensionsRegistry.byTypeAndAlias('workspace', UMB_BLOCK_WORKSPACE_ALIAS), (manifest) => {
			if (manifest) {
				createExtensionApi(this, manifest, [{ manifest: manifest }]).then((context) => {
					if (context) {
						this.#workspaceContext = context as typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
						this.#workspaceContext.establishLiveSync();
						this.#load();

						new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [
							this,
							this.#workspaceContext,
						]);
					}
				});
			}
		});
	}

	#load() {
		if (!this.#workspaceContext || !this.#contentKey) return;
		this.#workspaceContext.load(this.#contentKey);
	}

	override render() {
		return html`
			<div id="host">
				<button
					slot="header"
					id="open-part"
					tabindex="0"
					@keydown=${(e: KeyboardEvent) => {
						if (e.key !== ' ' && e.key !== 'Enter') return;
						e.preventDefault();
						e.stopPropagation();
						this._isOpen = !this._isOpen;
					}}
					@click=${() => {
						this._isOpen = !this._isOpen;
					}}>
					<uui-symbol-expand .open=${this._isOpen}></uui-symbol-expand>
					${this.#renderContent()}
					<slot></slot>
					<slot name="tag"></slot>
				</button>
				${this._isOpen === true
					? html`<umb-block-workspace-view-edit-content-no-router></umb-block-workspace-view-edit-content-no-router>`
					: ''}
			</div>
		`;
	}

	#renderContent() {
		return html`
			<span id="content">
				<span id="icon">
					<umb-icon .name=${this.icon}></umb-icon>
				</span>
				<div id="info">
					<div id="name">${this.label}</div>
				</div>
			</span>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#host {
				position: relative;
				display: block;
				width: 100%;

				box-sizing: border-box;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-surface);

				border: 1px solid var(--uui-color-border);
				transition: border-color 80ms;

				min-width: 250px;
			}
			#open-part + * {
				border-top: 1px solid var(--uui-color-border);
			}
			:host([disabled]) #open-part {
				cursor: default;
				transition: border-color 80ms;
			}
			:host(:not([disabled])) #host:has(#open-part:hover) {
				border-color: var(--uui-color-border-emphasis);
			}
			:host(:not([disabled])) #open-part:hover + * {
				border-color: var(--uui-color-border-emphasis);
			}
			:host([disabled]) #host {
				border-color: var(--uui-color-disabled-standalone);
			}

			slot[name='tag'] {
				flex-grow: 1;

				display: flex;
				justify-content: flex-end;
				align-items: center;
			}

			button {
				font-size: inherit;
				font-family: inherit;
				border: 0;
				padding: 0;
				background-color: transparent;
				text-align: left;
				color: var(--uui-color-text);
			}

			#content {
				align-self: stretch;
				line-height: normal;
				display: flex;
				position: relative;
				align-items: center;
			}

			#open-part {
				color: inherit;
				text-decoration: none;
				cursor: pointer;

				display: flex;
				text-align: left;
				align-items: center;
				justify-content: flex-start;
				width: 100%;
				border: none;
				background: none;

				min-height: var(--uui-size-16);
				padding: calc(var(--uui-size-2) + 1px);
			}

			#icon {
				font-size: 1.2em;
				margin-left: var(--uui-size-2);
				margin-right: var(--uui-size-1);
			}

			:host(:not([disabled])) #open-part:hover #icon {
				color: var(--uui-color-interactive-emphasis);
			}
			:host(:not([disabled])) #open-part:hover #name {
				color: var(--uui-color-interactive-emphasis);
			}

			:host([disabled]) #icon {
				color: var(--uui-color-disabled-contrast);
			}
			:host([disabled]) #name {
				color: var(--uui-color-disabled-contrast);
			}
		`,
	];
}

export default UmbInlineListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-inline-list-block': UmbInlineListBlockElement;
	}
}
