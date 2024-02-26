import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../media-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

import './media-workspace-view-edit-properties.element.js';
@customElement('umb-media-workspace-view-edit-tab')
export class UmbMediaWorkspaceViewEditTabElement extends UmbLitElement {
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

	_groupStructureHelper = new UmbContentTypeContainerStructureHelper<any>(this);

	@state()
	_groups: Array<PropertyTypeContainerModelBaseModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._groupStructureHelper.setStructureManager(workspaceContext.structure);
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
			${this._hasProperties
				? html`
						<uui-box>
							<umb-media-workspace-view-edit-properties
								class="properties"
								container-type="Tab"
								container-name=${this.tabName || ''}></umb-media-workspace-view-edit-properties>
						</uui-box>
				  `
				: ''}
			${repeat(
				this._groups,
				(group) => group.name,
				(group) =>
					html`<uui-box .headline=${group.name || ''}>
						<umb-media-workspace-view-edit-properties
							class="properties"
							container-type="Group"
							container-name=${group.name || ''}></umb-media-workspace-view-edit-properties>
					</uui-box>`,
			)}
		`;
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

export default UmbMediaWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-edit-tab': UmbMediaWorkspaceViewEditTabElement;
	}
}
