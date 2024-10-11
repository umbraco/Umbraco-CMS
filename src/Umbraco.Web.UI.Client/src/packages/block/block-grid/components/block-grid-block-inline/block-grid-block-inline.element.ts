import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from '../../context/block-grid-entry.context-token.js';
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

const apiArgsCreator: UmbApiConstructorArgumentsMethodType<unknown> = (manifest: unknown) => {
	return [{ manifest }];
};

/**
 * @element umb-block-grid-block-inline
 */
@customElement('umb-block-grid-block-inline')
export class UmbBlockGridBlockInlineElement extends UmbLitElement {
	//
	#blockContext?: typeof UMB_BLOCK_GRID_ENTRY_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#contentKey?: string;

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
	private _exposed?: boolean;

	@state()
	_isOpen = false;

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
						this.#workspaceContext.content.structure.contentTypeProperties,
						(contentTypeProperties) => {
							this._inlineProperty = contentTypeProperties[0];
						},
						'observeProperties',
					);

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
		return html`<umb-ref-grid-block
			standalone
			href=${(this.config?.showContentEdit ? this.config?.editContentPath : undefined) ?? ''}>
			<umb-icon slot="icon" .name=${this.icon}></umb-icon>
			<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
			${this.#renderInside()}
		</umb-ref-grid-block>`;
	}

	#renderInside() {
		if (this._exposed === false) {
			return html`<uui-button style="position:absolute; inset:0;" @click=${this.#expose}
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
		css`
			umb-block-grid-areas-container {
				margin-top: calc(var(--uui-size-2) + 1px);
			}
			umb-block-grid-areas-container::part(area) {
				margin: var(--uui-size-2);
			}
			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
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
