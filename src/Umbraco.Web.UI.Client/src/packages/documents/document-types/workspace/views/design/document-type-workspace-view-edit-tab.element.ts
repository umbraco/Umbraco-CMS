import type { UmbDocumentTypeDetailModel } from '../../../types.js';
import type { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import type { UmbDocumentTypeWorkspaceViewEditPropertiesElement } from './document-type-workspace-view-edit-properties.element.js';
import { css, html, customElement, property, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

import './document-type-workspace-view-edit-properties.element.js';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';

@customElement('umb-document-type-workspace-view-edit-tab')
export class UmbDocumentTypeWorkspaceViewEditTabElement extends UmbLitElement {
	#sorter = new UmbSorterController<
		PropertyTypeContainerModelBaseModel,
		UmbDocumentTypeWorkspaceViewEditPropertiesElement
	>(this, {
		getUniqueOfElement: (element) =>
			element.querySelector('umb-document-type-workspace-view-edit-properties')?.containerId ?? '',
		getUniqueOfModel: (modelEntry) => modelEntry.id,
		identifier: 'document-type-container-sorter',
		itemSelector: 'span',
		containerSelector: '#container-list',
		onChange: ({ item, model }) => {
			const modelIndex = model.findIndex((entry) => entry.id === item.id);
			if (modelIndex === -1) return;
			let sortOrder: number;

			if (model.length > 1) {
				sortOrder = modelIndex > 0 ? model[modelIndex - 1].sortOrder + 1 : model[modelIndex + 1].sortOrder - 1;
			} else {
				sortOrder = 0;
			}

			this._groupStructureHelper.partialUpdateContainer(item.id, {
				sortOrder: sortOrder,
			});

			this._groups = model;
		},
	});

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

	_groupStructureHelper = new UmbContentTypeContainerStructureHelper<UmbDocumentTypeDetailModel>(this);

	@state()
	_groups: Array<PropertyTypeContainerModelBaseModel> = [];

	@state()
	_hasProperties = false;

	@state()
	_sortModeActive?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this._groupStructureHelper.setStructureManager((context as UmbDocumentTypeWorkspaceContext).structure);
			this.observe(
				(context as UmbDocumentTypeWorkspaceContext).isSorting,
				(isSorting) => {
					this._sortModeActive = isSorting;
					if (isSorting) {
						this.#sorter.setModel(this._groups);
					} else {
						this.#sorter.setModel([]);
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
							<umb-document-type-workspace-view-edit-properties
								container-id=${ifDefined(this.ownerTabId === null ? undefined : this.ownerTabId)}
								container-type="Tab"
								container-name=${this.tabName || ''}></umb-document-type-workspace-view-edit-properties>
						</uui-box>
				  `
				: ''}
			<div id="container-list">
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
					<umb-document-type-workspace-view-edit-properties
						container-id=${ifDefined(group.id)}
						container-type="Group"
						container-name=${group.name || ''}></umb-document-type-workspace-view-edit-properties>
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

export default UmbDocumentTypeWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-edit-tab': UmbDocumentTypeWorkspaceViewEditTabElement;
	}
}
