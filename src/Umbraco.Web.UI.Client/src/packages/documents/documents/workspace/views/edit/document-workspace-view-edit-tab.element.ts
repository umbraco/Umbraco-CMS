import { UmbDocumentWorkspaceContext } from '../../document-workspace.context.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

import './document-workspace-view-edit-properties.element.js';
@customElement('umb-document-workspace-view-edit-tab')
export class UmbDocumentWorkspaceViewEditTabElement extends UmbLitElement {
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

	_groupStructureHelper = new UmbContentTypeContainerStructureHelper(this);

	@state()
	_groups: Array<PropertyTypeContainerResponseModelBaseModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._groupStructureHelper.setStructureManager((workspaceContext as UmbDocumentWorkspaceContext).structure);
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
							<umb-document-workspace-view-edit-properties
								container-type="Tab"
								container-name=${this.tabName || ''}></umb-document-workspace-view-edit-properties>
						</uui-box>
				  `
				: ''}
			${repeat(
				this._groups,
				(group) => group.name,
				(group) => html`<uui-box .headline=${group.name || ''}>
					<umb-document-workspace-view-edit-properties
						container-type="Group"
						container-name=${group.name || ''}></umb-document-workspace-view-edit-properties>
				</uui-box>`
			)}
		`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbDocumentWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-tab': UmbDocumentWorkspaceViewEditTabElement;
	}
}
