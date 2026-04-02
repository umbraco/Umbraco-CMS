import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '../../data-type-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-workspace-view-data-type-info')
export class UmbWorkspaceViewDataTypeInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _unique: string = '';

	@state()
	private _schemaAlias?: string;

	@state()
	private _uiAlias?: string | null;

	@state()
	private _dataSourceAlias?: string | null;

	private _workspaceContext?: typeof UMB_DATA_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (dataTypeContext) => {
			this._workspaceContext = dataTypeContext;
			this._observeDataType();
		});
	}

	private _observeDataType() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.unique, (unique) => {
			this._unique = unique!;
		});

		this.observe(this._workspaceContext.propertyEditorSchemaAlias, (schemaAlias) => {
			this._schemaAlias = schemaAlias;
		});

		this.observe(this._workspaceContext.propertyEditorUiAlias, (editorUiAlias) => {
			this._uiAlias = editorUiAlias;
		});

		this.observe(this._workspaceContext.propertyEditorDataSourceAlias, (dataSourceAlias) => {
			this._dataSourceAlias = dataSourceAlias;
		});
	}

	override render() {
		return html`
			<div class="container">
				<umb-extension-slot id="workspace-info-apps" type="workspaceInfoApp"></umb-extension-slot>
			</div>
			<div class="container">${this.#renderGeneralInfo()}</div>
		`;
	}

	#renderGeneralInfo() {
		return html`
			<uui-box id="general-section" headline="General">
				<div class="general-item">
					<strong><umb-localize key="template_id">Id</umb-localize></strong>
					<span>${this._unique}</span>
				</div>
				<div class="general-item">
					<strong>Property Editor Schema Alias</strong>
					<span>${this._schemaAlias}</span>
				</div>
				<div class="general-item">
					<strong>Property Editor UI Alias</strong>
					<span>${this._uiAlias}</span>
				</div>
				${this.#renderDataSourceInfo()}
			</uui-box>
		`;
	}

	#renderDataSourceInfo() {
		if (!this._dataSourceAlias) return nothing;

		return html`
			<div class="general-item">
				<strong>Property Editor Data Source Alias</strong>
				<span>${this._dataSourceAlias}</span>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
				grid-template-columns: 1fr 350px;
			}

			.container {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
			}

			#general-section {
				display: flex;
				flex-direction: column;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbWorkspaceViewDataTypeInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-data-type-info': UmbWorkspaceViewDataTypeInfoElement;
	}
}
