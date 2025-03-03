import { UMB_BLOCK_LIST_ENTRY_CONTEXT } from '../../context/index.js';
import {
	UMB_BLOCK_WORKSPACE_ALIAS,
	type UmbBlockDataType,
	type UMB_BLOCK_WORKSPACE_CONTEXT,
} from '@umbraco-cms/backoffice/block';
import {
	UmbExtensionApiInitializer,
	UmbExtensionsApiInitializer,
	type UmbApiConstructorArgumentsMethodType,
} from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../../../block/workspace/views/edit/block-workspace-view-edit-content-no-router.element.js';
import { UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

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

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@state()
	private _exposed?: boolean;

	@state()
	_isOpen = false;

	@state()
	private _ownerContentTypeName?: string;

	@state()
	private _variantName?: string;

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

		new UmbExtensionApiInitializer(
			this,
			umbExtensionsRegistry,
			UMB_BLOCK_WORKSPACE_ALIAS,
			apiArgsCreator,
			(permitted, ctrl) => {
				const context = ctrl.api as typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
				if (permitted && context) {
					this.#workspaceContext = context;
					this.#workspaceContext.establishLiveSync();
					this.#load();

					this.observe(
						this.#workspaceContext.exposed,
						(exposed) => {
							this._exposed = exposed;
						},
						'observeExposed',
					);

					this.observe(
						context.content.structure.ownerContentTypeName,
						(name) => {
							this._ownerContentTypeName = name;
						},
						'observeContentTypeName',
					);

					this.observe(
						context.variantId,
						async (variantId) => {
							if (variantId) {
								context.content.setup(this, variantId);
								// TODO: Support segment name?
								const culture = variantId.culture;
								if (culture) {
									const languageRepository = new UmbLanguageItemRepository(this);
									const { data } = await languageRepository.requestItems([culture]);
									const name = data?.[0].name;
									this._variantName = name ? this.localize.string(name) : undefined;
								}
							}
						},
						'observeVariant',
					);

					new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [this.#workspaceContext]);
				}
			},
		);
	}

	#load() {
		if (!this.#workspaceContext || !this.#contentKey) return;
		this.#workspaceContext.load(this.#contentKey);
	}

	#expose = () => {
		this.#workspaceContext?.expose();
	};

	override render() {
		return html`
			<div id="host">
				<button
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
					${this.#renderBlockInfo()}
					<slot></slot>
					<slot name="tag"></slot>
				</button>
				${this._isOpen === true ? this.#renderInside() : nothing}
			</div>
		`;
	}

	#renderBlockInfo() {
		return html`
			<span id="content">
				<span id="icon">
					<umb-icon .name=${this.icon}></umb-icon>
				</span>
				<div id="info">
					<umb-ufm-render id="name" inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
				</div>
			</span>
			${this.unpublished
				? html`<uui-tag slot="name" look="secondary" title=${this.localize.term('blockEditor_notExposedDescription')}
						><umb-localize key="blockEditor_notExposedLabel"></umb-localize
					></uui-tag>`
				: nothing}
		`;
	}

	#renderInside() {
		if (this._exposed === false) {
			return html`<uui-button id="exposeButton" draggable="false" @click=${this.#expose}
				><uui-icon name="icon-add"></uui-icon>
				<umb-localize
					key="blockEditor_createThisFor"
					.args=${[this._ownerContentTypeName, this._variantName]}></umb-localize
			></uui-button>`;
		} else {
			return html`<umb-block-workspace-view-edit-content-no-router
				draggable="false"></umb-block-workspace-view-edit-content-no-router>`;
		}
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

			#exposeButton {
				width: 100%;
				min-height: var(--uui-size-16);
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

			:host([unpublished]) #open-part #content {
				opacity: 0.6;
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

			#info {
				display: flex;
				flex-direction: column;
				align-items: start;
				justify-content: center;
				height: 100%;
				padding-left: var(--uui-size-2, 6px);
			}

			#name {
				font-weight: 700;
			}

			uui-tag {
				margin-left: 0.5em;
				margin-bottom: -0.3em;
				margin-top: -0.3em;
				vertical-align: text-top;
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
