import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbWorkspaceDataTypeContext } from '../../workspace-data-type.context';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-workspace-view-data-type-info')
export class UmbWorkspaceViewDataTypeInfoElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@state()
	_dataType?: DataTypeDetails;

	private _workspaceContext?: UmbWorkspaceDataTypeContext;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (dataTypeContext) => {
			this._workspaceContext = dataTypeContext;
			this._observeDataType();
		});
	}

	private _observeDataType() {
		if (!this._workspaceContext) return;

		this.observe<DataTypeDetails>(this._workspaceContext.data.pipe(distinctUntilChanged()), (dataType) => {
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
				<umb-workspace-property-layout
					label="Property Editor Alias">
					<div slot="editor">${this._dataType?.propertyEditorModelAlias}</div>
				</umb-workspace-property-layout>

				<umb-workspace-property-layout
					label="Property Editor UI Alias">
					<div slot="editor">${this._dataType?.propertyEditorUIAlias}</div>
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
