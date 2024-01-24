import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper, UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

import './block-workspace-view-edit-properties.element.js';

@customElement('umb-block-workspace-view-edit-tab')
export class UmbBlockWorkspaceViewEditTabElement extends UmbLitElement {
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

	@property({ type: Boolean })
	public get noTabName(): boolean {
		return this._groupStructureHelper.getIsRoot();
	}
	public set noTabName(value: boolean) {
		this._groupStructureHelper.setIsRoot(value);
	}

	private _ownerTabId?: string | null;
	@property({ type: String })
	public get ownerTabId(): string | null | undefined {
		return this._ownerTabId;
	}
	public set ownerTabId(value: string | null | undefined) {
		if (value === this._ownerTabId) return;
		this._ownerTabId = value;
		this._groupStructureHelper.setOwnerId(value);
	}

	/**
	 * If true, the group box will be hidden, if we are to only represents one group.
	 * This is used by Inline Editing Mode of Block List Editor.
	 */
	@property({ type: Boolean, reflect: false })
	hideSingleGroup = false;

	_groupStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);

	@state()
	_groups: Array<PropertyTypeContainerModelBaseModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._groupStructureHelper.setStructureManager(workspaceContext.content.structure);
		});
		this.observe(this._groupStructureHelper.containers, (groups) => {
			this._groups = groups;
		});
		this.observe(this._groupStructureHelper.hasProperties, (hasProperties) => {
			this._hasProperties = hasProperties;
		});
	}

	render() {
		return html`
			${this._hasProperties ? this.#renderPart(this._tabName) : ''}
			${repeat(
				this._groups,
				(group) => group.name,
				(group) => this.#renderPart(group.name, group.name),
			)}
		`;
	}

	#renderPart(groupName: string | null | undefined, boxName?: string | null | undefined) {
		return this.hideSingleGroup && this._groups.length === 1
			? html` <umb-block-workspace-view-edit-properties
					class="properties"
					container-type="Group"
					container-name=${groupName || ''}></umb-block-workspace-view-edit-properties>`
			: html` <uui-box .headline=${boxName || ''}
					><umb-block-workspace-view-edit-properties
						class="properties"
						container-type="Group"
						container-name=${groupName || ''}></umb-block-workspace-view-edit-properties
			  ></uui-box>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-box {
				--uui-box-default-padding: 0 var(--uui-size-space-5);
			}
			uui-box:not(:first-child) {
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbBlockWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-tab': UmbBlockWorkspaceViewEditTabElement;
	}
}
