import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import {
	UMB_BLOCK_CATALOGUE_MODAL,
	type UmbBlockLayoutBaseModel,
	type UmbBlockTypeBaseModel,
	type UmbBlockTypeGroup,
} from '@umbraco-cms/backoffice/block';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';

/**
 * @element umb-property-editor-ui-block-grid
 */
@customElement('umb-property-editor-ui-block-grid')
export class UmbPropertyEditorUIBlockGridElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	@state()
	private _blocks?: Array<UmbBlockTypeBaseModel>;

	@state()
	private _blockGroups?: Array<UmbBlockTypeGroup>;

	@state()
	private _layouts: Array<UmbBlockLayoutBaseModel> = [];

	@state()
	private _catalogueRouteBuilder?: UmbModalRouteBuilder;

	@state()
	private _directRoute?: string;

	@state()
	private _createButtonLabel = this.localize.term('content_createEmpty');

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const validationLimit = config.getValueByAlias<NumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min;
		this._limitMax = validationLimit?.max;

		this._blocks = config.getValueByAlias<Array<UmbBlockTypeBaseModel>>('blocks') ?? [];
		this._blockGroups = config.getValueByAlias<Array<UmbBlockTypeGroup>>('blockGroups') ?? [];

		const customCreateButtonLabel = config.getValueByAlias<string>('createLabel');
		if (customCreateButtonLabel) {
			this._createButtonLabel = customCreateButtonLabel;
		} else if (this._blocks.length === 1) {
			this._createButtonLabel = `${this.localize.term('general_add')} ${this._blocks[0].label}`;
		}

		//const useInlineEditingAsDefault = config.getValueByAlias<boolean>('useInlineEditingAsDefault');

		//this.#context.setInlineEditingMode(useInlineEditingAsDefault);
		//config.useSingleBlockMode
		//config.useLiveEditing
		//config.useInlineEditingAsDefault
		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';

		//this.#context.setEditorConfiguration(config);
	}

	constructor() {
		super();

		/*
		// TODO: Prevent initial notification from these observes:
		this.observe(this.#context.layouts, (layouts) => {
			this._value = { ...this._value, layout: { [UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS]: layouts } };
			// Notify that the value has changed.
			//console.log('layout changed', this._value);
			// TODO: idea: consider inserting an await here, so other changes could appear first? Maybe some mechanism to only fire change event onces?
			this._layouts = layouts;
			this.#sorter.setModel(layouts);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#context.contents, (contents) => {
			this._value = { ...this._value, contentData: contents };
			// Notify that the value has changed.
			//console.log('content changed', this._value);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#context.settings, (settings) => {
			this._value = { ...this._value, settingsData: settings };
			// Notify that the value has changed.
			//console.log('settings changed', this._value);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#context.blockTypes, (blockTypes) => {
			this._blocks = blockTypes;
		});
		*/

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addAdditionalPath(':view/:index')
			.onSetup((routingInfo) => {
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				return {
					data: {
						blocks: this._blocks ?? [],
						blockGroups: this._blockGroups ?? [],
						openClipboard: routingInfo.view === 'clipboard',
						blockOriginData: { index: index },
					},
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				this._catalogueRouteBuilder = routeBuilder;
			});
	}

	render() {
		if (this._blocks?.length === 1) {
			const elementKey = this._blocks[0].contentElementTypeKey;
			this._directRoute =
				this._catalogueRouteBuilder?.({ view: 'create', index: -1 }) + 'modal/umb-modal-workspace/create/' + elementKey;
		}
		return html`<uui-button-group>
			<uui-button
				id="add-button"
				look="placeholder"
				label=${this._createButtonLabel}
				href=${this._directRoute ?? this._catalogueRouteBuilder?.({ view: 'create', index: -1 }) ?? ''}></uui-button>
			<uui-button
				label=${this.localize.term('content_createFromClipboard')}
				look="placeholder"
				href=${this._catalogueRouteBuilder?.({ view: 'clipboard', index: -1 }) ?? ''}>
				<uui-icon name="icon-paste-in"></uui-icon>
			</uui-button>
		</uui-button-group>`;
	}

	/*
setupRoutes() {
	this._routes = [];
	if (this._variantId !== undefined) {
		this._routes = [
			{
				path: 'modal-1',
				component: () => {
					return import('./property-editor-ui-block-grid-inner-test.element.js');
				},
				setup: (component) => {
					if (component instanceof HTMLElement) {
						(component as any).name = 'block-grid-1';
					}
				},
			},
			{
				path: 'modal-2',
				component: () => {
					return import('./property-editor-ui-block-grid-inner-test.element.js');
				},
				setup: (component) => {
					if (component instanceof HTMLElement) {
						(component as any).name = 'block-grid-2';
					}
				},
			},
		];
	}
}
*/
	/*
	render() {
		return this._variantId
			? html`<div>
					umb-property-editor-ui-block-grid, inner routing test:

					<uui-tab-group slot="navigation">
						<uui-tab
							label="TAB 1"
							href="${this._routerPath + '/'}modal-1"
							.active=${this._routerPath + '/' + 'modal-1' === this._activePath}></uui-tab>
						<uui-tab
							label="TAB 2"
							href="${this._routerPath + '/'}modal-2"
							.active=${this._routerPath + '/' + 'modal-2' === this._activePath}></uui-tab>
					</uui-tab-group>

					<umb-variant-router-slot
						.variantId=${[this._variantId]}
						id="router-slot"
						.routes="${this._routes}"
						@init=${(event: UmbRouterSlotInitEvent) => {
							this._routerPath = event.target.absoluteRouterPath;
						}}
						@change=${(event: UmbRouterSlotChangeEvent) => {
							this._activePath = event.target.localActiveViewPath;
						}}>
					</umb-variant-router-slot>
			  </div>`
			: 'loading...';
	}
	*/

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				gap: 1px;
			}
			> div {
				display: flex;
				flex-direction: column;
				align-items: stretch;
			}

			uui-button-group {
				padding-top: 1px;
				display: grid;
				grid-template-columns: 1fr auto;
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid': UmbPropertyEditorUIBlockGridElement;
	}
}
