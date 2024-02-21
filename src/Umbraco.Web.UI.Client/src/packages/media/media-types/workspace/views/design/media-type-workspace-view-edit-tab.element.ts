import type { UmbMediaTypeWorkspaceContext } from '../../media-type-workspace.context.js';
import type { UmbMediaTypeDetailModel } from '../../../types.js';
import { css, html, customElement, property, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';

import './media-type-workspace-view-edit-properties.element.js';

const SORTER_CONFIG: UmbSorterConfig<PropertyTypeContainerModelBaseModel> = {
	getUniqueOfElement: (element) => {
		return element.getAttribute('data-umb-group-id');
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.id;
	},
	identifier: 'content-type-group-sorter',
	itemSelector: '[data-umb-group-id]',
	disabledItemSelector: '[inherited]',
	containerSelector: '#group-list',
};

@customElement('umb-media-type-workspace-view-edit-tab')
export class UmbMediaTypeWorkspaceViewEditTabElement extends UmbLitElement {
	public sorter?: UmbSorterController<PropertyTypeContainerModelBaseModel>;

	config: UmbSorterConfig<PropertyTypeContainerModelBaseModel> = {
		...SORTER_CONFIG,
		// TODO: Missing handlers to work properly: performItemMove and performItemRemove
		performItemInsert: async (args) => {
			if (!this._groups) return false;
			const oldIndex = this._groups.findIndex((group) => group.id! === args.item.id);
			if (args.newIndex === oldIndex) return true;

			let sortOrder = 0;
			//TODO the sortOrder set is not correct
			if (this._groups.length > 0) {
				if (args.newIndex === 0) {
					sortOrder = (this._groups[0].sortOrder ?? 0) - 1;
				} else {
					sortOrder = (this._groups[Math.min(args.newIndex, this._groups.length - 1)].sortOrder ?? 0) + 1;
				}

				if (sortOrder !== args.item.sortOrder) {
					await this._groupStructureHelper.partialUpdateContainer(args.item.id!, { sortOrder });
				}
			}

			return true;
		},
	};

	private _ownerTabId?: string | null;

	// TODO: get rid of this:
	@property({ type: String })
	public get ownerTabId(): string | null | undefined {
		return this._ownerTabId;
	}
	public set ownerTabId(value: string | null | undefined) {
		if (value === this._ownerTabId) return;
		const oldValue = this._ownerTabId;
		this._ownerTabId = value;
		this._groupStructureHelper.setOwnerId(value);
		this.requestUpdate('ownerTabId', oldValue);
	}

	private _tabName?: string | undefined;

	@property({ type: String })
	public get tabName(): string | undefined {
		return this._groupStructureHelper.getName();
	}
	public set tabName(value: string | undefined) {
		if (value === this._tabName) return;
		const oldValue = this._tabName;
		this._tabName = value;
		this._groupStructureHelper.setName(value);
		this.requestUpdate('tabName', oldValue);
	}

	@state()
	private _noTabName?: boolean;

	@property({ type: Boolean })
	public get noTabName(): boolean {
		return this._groupStructureHelper.getIsRoot();
	}
	public set noTabName(value: boolean) {
		this._noTabName = value;
		this._groupStructureHelper.setIsRoot(value);
	}

	_groupStructureHelper = new UmbContentTypeContainerStructureHelper<UmbMediaTypeDetailModel>(this);

	@state()
	_groups: Array<PropertyTypeContainerModelBaseModel> = [];

	@state()
	_hasProperties = false;

	@state()
	_sortModeActive?: boolean;

	constructor() {
		super();

		this.sorter = new UmbSorterController(this, this.config);

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this._groupStructureHelper.setStructureManager((context as UmbMediaTypeWorkspaceContext).structure);
			this.observe(
				(context as UmbMediaTypeWorkspaceContext).isSorting,
				(isSorting) => {
					this._sortModeActive = isSorting;
					if (isSorting) {
						this.sorter?.setModel(this._groups);
					} else {
						this.sorter?.setModel([]);
					}
				},
				'_observeIsSorting',
			);
		});
		this.observe(this._groupStructureHelper.containers, (groups) => {
			this._groups = groups;
			this.requestUpdate('_groups');
		});
		this.observe(this._groupStructureHelper.hasProperties, (hasProperties) => {
			this._hasProperties = hasProperties;
			this.requestUpdate('_hasProperties');
		});
	}

	#onAddGroup = () => {
		// Idea, maybe we can gather the sortOrder from the last group rendered and add 1 to it?
		this._groupStructureHelper.addContainer(this._ownerTabId);
	};

	render() {
		return html`
			${!this._noTabName
				? html`
						<uui-box>
							<umb-media-type-workspace-view-edit-properties
								container-id=${ifDefined(this.ownerTabId === null ? undefined : this.ownerTabId)}
								container-type="Tab"
								container-name=${this.tabName || ''}></umb-media-type-workspace-view-edit-properties>
						</uui-box>
				  `
				: ''}
			<div id="group-list">
				${repeat(
					this._groups,
					(group) => group.id ?? '' + group.name,
					(group) => html`<span data-umb-group-id=${ifDefined(group.id)}>
					<uui-box>
						${
							this._groupStructureHelper.isOwnerChildContainer(group.id!)
								? html`
										<div slot="header">
											<div>
												${this._sortModeActive ? html`<uui-icon name="icon-navigation"></uui-icon>` : ''}

												<uui-input
													label="Group name"
													placeholder="Enter a group name"
													value=${group.name ?? ''}
													@change=${(e: InputEvent) => {
														const newName = (e.target as HTMLInputElement).value;
														this._groupStructureHelper.updateContainerName(
															group.id!,
															group.parent?.id ?? null,
															newName,
														);
													}}>
												</uui-input>
											</div>
											${this._sortModeActive
												? html`<uui-input type="number" label="sort order" .value=${group.sortOrder ?? 0}></uui-input>`
												: ''}
										</div>
								  `
								: html`<div slot="header">
										<div><uui-icon name="icon-merge"></uui-icon><b>${group.name ?? ''}</b> (Inherited)</div>
										${!this._sortModeActive
											? html`<uui-input
													readonly
													type="number"
													label="sort order"
													.value=${group.sortOrder ?? 0}></uui-input>`
											: ''}
								  </div>`
						}
					</div>
					<umb-media-type-workspace-view-edit-properties
						container-id=${ifDefined(group.id)}
						container-type="Group"
						container-name=${group.name || ''}></umb-media-type-workspace-view-edit-properties>
				</uui-box></span>`,
				)}
			</div>
			${!this._sortModeActive
				? html`<uui-button
						label=${this.localize.term('contentTypeEditor_addGroup')}
						id="add"
						look="placeholder"
						@click=${this.#onAddGroup}>
						${this.localize.term('contentTypeEditor_addGroup')}
				  </uui-button>`
				: ''}
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#add {
				width: 100%;
			}

			#add:first-child {
				margin-top: var(--uui-size-layout-1);
			}
			uui-box {
				margin-bottom: var(--uui-size-layout-1);
			}

			[data-umb-group-id] {
				display: block;
				position: relative;
			}

			div[slot='header'] {
				display: flex;
				align-items: center;
				justify-content: space-between;
			}

			div[slot='header'] > div {
				display: flex;
				align-items: center;
				gap: var(--uui-size-3);
			}

			uui-input[type='number'] {
				max-width: 75px;
			}

			.sorting {
				cursor: grab;
			}

			.--umb-sorter-placeholder > uui-box {
				visibility: hidden;
			}

			.--umb-sorter-placeholder::after {
				content: '';
				inset: 0;
				position: absolute;
				border-radius: var(--uui-border-radius);
				border: 1px dashed var(--uui-color-divider-emphasis);
			}
		`,
	];
}

export default UmbMediaTypeWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-view-edit-tab': UmbMediaTypeWorkspaceViewEditTabElement;
	}
}
