import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '../../data-type-workspace.context-token.js';
import type { UmbDataTypeDetailModel } from '../../../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-workspace-view-data-type-info')
export class UmbWorkspaceViewDataTypeInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	_dataType?: UmbDataTypeDetailModel;

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
				<umb-property-layout label="Id">
					<div slot="editor">${this._dataType?.unique}</div>
				</umb-property-layout>
				<umb-property-layout label="Property Editor Alias">
					<div slot="editor">${this._dataType?.editorAlias}</div>
				</umb-property-layout>

				<umb-property-layout label="Property Editor UI Alias">
					<div slot="editor">${this._dataType?.editorUiAlias}</div>
				</umb-property-layout>
			</uui-box>
		`;
	}

	private _renderReferences() {
		return html` <uui-box headline="References"> </uui-box> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
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
