import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import { css, html, customElement, property, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

import './document-type-workspace-view-edit-properties.element.js';

@customElement('umb-document-type-workspace-view-edit-tab')
export class UmbDocumentTypeWorkspaceViewEditTabElement extends UmbLitElement {
	private _ownerTabId?: string | null;

	// TODO: get rid of this:
	@property({ type: String })
	public get ownerTabId(): string | null | undefined {
		return this._ownerTabId;
	}
	public set ownerTabId(value: string | null | undefined) {
		if (value === this._ownerTabId) return;
		console.log('ownerTabId', value);
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

	_groupStructureHelper = new UmbContentTypeContainerStructureHelper(this);

	@state()
	_groups: Array<PropertyTypeContainerModelBaseModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this._groupStructureHelper.setStructureManager((context as UmbDocumentTypeWorkspaceContext).structure);
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
			${repeat(
				this._groups,
				(group) => group.id ?? '' + group.name,
				(group) => html`
					<uui-box>
						${
							this._groupStructureHelper.isOwnerChildContainer(group.id!)
								? html`
										<div slot="header">
											<uui-input
												label="Group name"
												placeholder="Enter a group name"
												value=${group.name ?? ''}
												@change=${(e: InputEvent) => {
													const newName = (e.target as HTMLInputElement).value;
													this._groupStructureHelper.updateContainerName(group.id!, group.parentId ?? null, newName);
												}}>
											</uui-input>
										</div>
								  `
								: html`<div slot="header"><b>${group.name ?? ''}</b> (Inherited)</div>`
						}
					</div>
					<umb-document-type-workspace-view-edit-properties
						container-id=${ifDefined(group.id)}
						container-type="Group"
						container-name=${group.name || ''}></umb-document-type-workspace-view-edit-properties>
				</uui-box>`,
			)}
			<uui-button
				label=${this.localize.term('contentTypeEditor_addGroup')}
				id="add"
				look="placeholder"
				@click=${this.#onAddGroup}>
				${this.localize.term('contentTypeEditor_addGroup')}
			</uui-button>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			#add {
				width: 100%;
			}

			#add:not(:first-child) {
				width: 100%;
				margin-top: var(--uui-size-layout-1);
			}
			uui-box:not(:first-child) {
				margin-top: var(--uui-size-layout-1);
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
