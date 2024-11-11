import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from '../../context/block-grid-entry.context-token.js';
import type { UmbBlockGridWorkspaceOriginData } from '../../workspace/block-grid-workspace.modal-token.js';
import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from '../../context/block-grid-entries.context-token.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import '../block-grid-areas-container/index.js';
import '../ref-grid-block/index.js';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';
import {
	type UMB_BLOCK_WORKSPACE_CONTEXT,
	UMB_BLOCK_WORKSPACE_ALIAS,
	type UmbBlockDataType,
} from '@umbraco-cms/backoffice/block';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbExtensionApiInitializer,
	UmbExtensionsApiInitializer,
	type UmbApiConstructorArgumentsMethodType,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

@customElement('umb-block-grid-block-inline')
export class UmbBlockGridBlockInlineElement extends UmbLitElement {
	//
	#blockContext?: typeof UMB_BLOCK_GRID_ENTRY_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#contentKey?: string;
	#parentUnique?: string | null;
	#areaKey?: string | null;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	@property({ type: String, reflect: false })
	label?: string;

	@property({ type: String, reflect: false })
	icon?: string;

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@state()
	_inlineProperty?: UmbPropertyTypeModel;

	@state()
	private _ownerContentTypeName?: string;

	@state()
	private _variantName?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_GRID_ENTRY_CONTEXT, (blockContext) => {
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
		this.consumeContext(UMB_BLOCK_GRID_ENTRIES_CONTEXT, (entriesContext) => {
			this.#parentUnique = entriesContext.getParentUnique();
			this.#areaKey = entriesContext.getAreaKey();
		});
		new UmbExtensionApiInitializer(
			this,
			umbExtensionsRegistry,
			UMB_BLOCK_WORKSPACE_ALIAS,
			apiArgsCreator,
			(permitted, ctrl) => {
				const context = ctrl.api as typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
				if (permitted && context) {
					// Risky business, cause here we are lucky that it seems to be consumed and set before this is called and there for this is acceptable for now. [NL]
					if (this.#parentUnique === undefined || this.#areaKey === undefined) {
						throw new Error('Parent unique and area key must be defined');
					}
					this.#workspaceContext = context;
					context.setOriginData({
						areaKey: this.#areaKey,
						parentUnique: this.#parentUnique,
					} as UmbBlockGridWorkspaceOriginData);
					this.#workspaceContext.establishLiveSync();

					this.#load();

					this.observe(
						this.#workspaceContext.content.structure.contentTypeProperties,
						(contentTypeProperties) => {
							this._inlineProperty = contentTypeProperties[0];
						},
						'observeProperties',
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

					new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [
						this,
						this.#workspaceContext,
					]);
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
				<button id="open-part" tabindex="0">
					${this.#renderBlockInfo()}
					<slot></slot>
					<slot name="tag"></slot>
				</button>
				<div id="inside">${this.#renderInside()}</div>
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
		`;
	}

	#renderInside() {
		if (this.unpublished === true) {
			return html`<uui-button id="exposeButton" @click=${this.#expose}
				><uui-icon name="icon-add"></uui-icon>
				<umb-localize
					key="blockEditor_createThisFor"
					.args=${[this._ownerContentTypeName, this._variantName]}></umb-localize
			></uui-button>`;
		} else {
			return html`<umb-property-type-based-property
					.property=${this._inlineProperty}
					slot="areas"></umb-property-type-based-property>
				<umb-block-grid-areas-container slot="areas"></umb-block-grid-areas-container>`;
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			umb-block-grid-areas-container {
				margin-top: calc(var(--uui-size-2) + 1px);
			}
			umb-block-grid-areas-container::part(area) {
				margin: var(--uui-size-2);
			}

			#exposeButton {
				width: 100%;
				min-height: var(--uui-size-layout-3);
			}

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

			:host([unpublished]) #open-part {
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

			#inside {
				position: relative;
				display: block;
				padding: calc(var(--uui-size-layout-1));
			}
		`,
	];
}

export default UmbBlockGridBlockInlineElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-block-inline': UmbBlockGridBlockInlineElement;
	}
}
