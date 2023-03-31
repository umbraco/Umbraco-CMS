import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDataTypeWorkspaceContext } from '../../data-type-workspace.context';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-workspace-view-data-type-info')
export class UmbWorkspaceViewDataTypeInfoElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeResponseModel;

	private _workspaceContext?: UmbDataTypeWorkspaceContext;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (dataTypeContext) => {
			this._workspaceContext = dataTypeContext as UmbDataTypeWorkspaceContext;
			this._observeDataType();
		});
	}

	private _observeDataType() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.data, (dataType) => {
			if (!dataType) return;
			this._dataType = dataType;
		});
	}

	render() {
		return html` ${this._renderGeneralInfo()}${this._renderReferences()} `;
	}

	private _renderGeneralInfo() {
		return html`
			<uui-box headline="General" style="margin-bottom: 20px;">
				<umb-workspace-property-layout label="Key">
					<div slot="editor">${this._dataType?.key}</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="Property Editor Alias">
					<div slot="editor">${this._dataType?.propertyEditorAlias}</div>
				</umb-workspace-property-layout>

				<umb-workspace-property-layout label="Property Editor UI Alias">
					<div slot="editor">${this._dataType?.propertyEditorUiAlias}</div>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}

	private _renderReferences() {
		return html` <uui-box headline="References"> </uui-box> `;
	}
}

export default UmbWorkspaceViewDataTypeInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-data-type-info': UmbWorkspaceViewDataTypeInfoElement;
	}
}
