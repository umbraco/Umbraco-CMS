import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import {
	UMB_BLOCK_CATALOGUE_MODAL,
	type UmbBlockLayoutBaseModel,
	type UmbBlockTypeBaseModel,
	type UmbBlockTypeGroup,
} from '@umbraco-cms/backoffice/block';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

/**
 * @element umb-property-editor-ui-block-grid
 */
@customElement('umb-property-editor-ui-block-grid')
export class UmbPropertyEditorUIBlockGridElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#catalogueModal: UmbModalRouteRegistrationController<typeof UMB_BLOCK_CATALOGUE_MODAL.DATA, undefined>;

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
	private _createButtonLabel = this.localize.term('blockEditor_addBlock');

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
			this._createButtonLabel = this.localize.term('blockEditor_addThis', [this._blocks[0].label]);
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

		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.observe(
				propertyContext?.alias,
				(alias) => {
					this.#catalogueModal.setUniquePathValue('propertyAlias', alias);
				},
				'observePropertyAlias',
			);
		});

		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias'])
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
