import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../document-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

import './document-workspace-view-edit-properties.element.js';
@customElement('umb-document-workspace-view-edit-tab')
export class UmbDocumentWorkspaceViewEditTabElement extends UmbLitElement {
	@property({ type: String })
	public get tabName(): string | undefined {
		return this.#groupStructureHelper.getName();
	}
	public set tabName(value: string | undefined) {
		const oldName = this.#groupStructureHelper.getName();
		this.#groupStructureHelper.setName(value);
		this.requestUpdate('tabName', oldName);
	}

	@property({ type: Boolean })
	public get noTabName(): boolean {
		return this.#groupStructureHelper.getIsRoot();
	}
	public set noTabName(value: boolean) {
		this.#groupStructureHelper.setIsRoot(value);
	}

	@property({ type: String })
	public get containerId(): string | null | undefined {
		return this.#groupStructureHelper.getParentId();
	}
	public set containerId(value: string | null | undefined) {
		this.#groupStructureHelper.setParentId(value);
	}

	#groupStructureHelper = new UmbContentTypeContainerStructureHelper<any>(this);

	@state()
	_groups: Array<PropertyTypeContainerModelBaseModel> = [];

	@state()
	_hasProperties = false;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#groupStructureHelper.setStructureManager(workspaceContext.structure);
		});
		this.observe(this.#groupStructureHelper.containers, (groups) => {
			this._groups = groups;
		});
		this.observe(this.#groupStructureHelper.hasProperties, (hasProperties) => {
			this._hasProperties = hasProperties;
		});
	}

	render() {
		return html`
			${this._hasProperties
				? html`
						<uui-box>
							<umb-document-workspace-view-edit-properties
								class="properties"
								container-type="Tab"
								container-name=${this.tabName ?? ''}></umb-document-workspace-view-edit-properties>
						</uui-box>
					`
				: ''}
			${repeat(
				this._groups,
				(group) => group.id,
				(group) =>
					html`<uui-box .headline=${group.name ?? ''}>
						<umb-document-workspace-view-edit-properties
							class="properties"
							container-type="Group"
							container-name=${group.name ?? ''}></umb-document-workspace-view-edit-properties>
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

export default UmbDocumentWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-tab': UmbDocumentWorkspaceViewEditTabElement;
	}
}
