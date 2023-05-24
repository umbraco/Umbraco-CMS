import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

import './document-type-workspace-view-edit-properties.element.js';

@customElement('umb-document-type-workspace-view-edit-tab')
export class UmbDocumentTypeWorkspaceViewEditTabElement extends UmbLitElement {
	private _ownerTabId?: string | undefined;

	@property({ type: String })
	public get ownerTabId(): string | undefined {
		return this._ownerTabId;
	}
	public set ownerTabId(value: string | undefined) {
		if (value === this._ownerTabId) return;
		const oldValue = this._ownerTabId;
		this._ownerTabId = value;
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

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this._groupStructureHelper.setStructureManager((context as UmbDocumentTypeWorkspaceContext).structure);
		});
		this.observe(this._groupStructureHelper.containers, (groups) => {
			this._groups = groups;
		});
		this.observe(this._groupStructureHelper.hasProperties, (hasProperties) => {
			this._hasProperties = hasProperties;
		});
	}

	#onAddGroup = () => {
		// Idea, maybe we can gather the sortOrder from the last group rendered and add 1 to it?
		this._groupStructureHelper.addContainer(this._ownerTabId);
	};

	render() {
		return html`
			${this._hasProperties
				? html`
						<uui-box>
							<umb-document-type-workspace-view-edit-properties
								container-id=${this.ownerTabId}
								container-type="Tab"
								container-name=${this.tabName || ''}></umb-document-type-workspace-view-edit-properties>
						</uui-box>
				  `
				: ''}
			${repeat(
				this._groups,
				(group) => group.name,
				(group) => html`<uui-box .headline=${group.name || ''}>
					<umb-document-type-workspace-view-edit-properties
						container-id=${group.id}
						container-type="Group"
						container-name=${group.name || ''}></umb-document-type-workspace-view-edit-properties>
				</uui-box>`
			)}
			<uui-button id="add" look="placeholder" @click=${this.#onAddGroup}> Add Group </uui-button>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-box {
				margin: var(--uui-size-layout-1);
			}

			#add {
				width: 100%;
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
