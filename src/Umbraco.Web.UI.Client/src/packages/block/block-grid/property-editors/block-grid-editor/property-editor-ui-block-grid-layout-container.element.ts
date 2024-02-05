import {
	UMB_BLOCK_CATALOGUE_MODAL,
	type UmbBlockGridLayoutModel,
	type UmbBlockTypeBaseModel,
} from '@umbraco-cms/backoffice/block';
import { html, customElement, state, repeat, css, property } from '@umbraco-cms/backoffice/external/lit';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-grid-entries
 */
@customElement('umb-property-editor-ui-block-grid-entries')
export class UmbPropertyEditorUIBlockGridEntriesElement extends UmbLitElement {
	// TODO: Make sure Sorter handles columnSpan when retrieving a new entry.
	#catalogueModal: UmbModalRouteRegistrationController<typeof UMB_BLOCK_CATALOGUE_MODAL.DATA, undefined>;

	@state()
	private _catalogueRouteBuilder?: UmbModalRouteBuilder;

	@state()
	private _blocks?: Array<UmbBlockTypeBaseModel>;

	@property({ attribute: false })
	public set layoutEntries(value: Array<UmbBlockGridLayoutModel>) {
		this._layoutEntries = value;
	}
	public get layoutEntries(): Array<UmbBlockGridLayoutModel> {
		return this._layoutEntries;
	}
	private _layoutEntries: Array<UmbBlockGridLayoutModel> = [];

	@state()
	private _createButtonLabel = this.localize.term('content_createEmpty');

	constructor() {
		super();

		// TODO: Observe Blocks of the layout entries of this component.

		// TODO: Could this become part of the Block Manager Context?
		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.observe(
				propertyContext?.alias,
				(alias) => {
					this.#catalogueModal.setUniquePathValue('propertyAlias', alias);
				},
				'observePropertyAlias',
			);
		});

		// Maybe this can be moved to the Block Manager Context? As I don't want to know about groups here. Maybe just part of the onSetup method?
		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias', 'parentUnique'])
			.addAdditionalPath(':view/:index')
			.onSetup((routingInfo) => {
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				return {
					data: {
						blocks: this._blocks ?? [],
						//blockGroups: this._blockGroups ?? [],
						blockGroups: [],
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
		let createPath: string | undefined;
		if (this._blocks?.length === 1) {
			const elementKey = this._blocks[0].contentElementTypeKey;
			createPath =
				this._catalogueRouteBuilder?.({ view: 'create', index: -1 }) + 'modal/umb-modal-workspace/create/' + elementKey;
		} else {
			createPath = this._catalogueRouteBuilder?.({ view: 'create', index: -1 });
		}
		return html` ${repeat(
				this._layoutEntries,
				(x) => x.contentUdi,
				(layoutEntry, index) =>
					html`<uui-button-inline-create
							href=${this._catalogueRouteBuilder?.({ view: 'create', index: index }) ?? ''}></uui-button-inline-create>
						<umb-property-editor-ui-block-list-block data-udi=${layoutEntry.contentUdi} .layout=${layoutEntry}>
						</umb-property-editor-ui-block-list-block> `,
			)}
			<uui-button-group>
				<uui-button
					id="add-button"
					look="placeholder"
					label=${this._createButtonLabel}
					href=${createPath ?? ''}></uui-button>
				<uui-button
					label=${this.localize.term('content_createFromClipboard')}
					look="placeholder"
					href=${this._catalogueRouteBuilder?.({ view: 'clipboard', index: -1 }) ?? ''}>
					<uui-icon name="icon-paste-in"></uui-icon>
				</uui-button>
			</uui-button-group>`;
		//
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

export default UmbPropertyEditorUIBlockGridEntriesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-entries': UmbPropertyEditorUIBlockGridEntriesElement;
	}
}
