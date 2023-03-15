import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbRelationTypeWorkspaceContext } from '../../relation-type-workspace.context';

import { UmbLitElement } from '@umbraco-cms/element';
import { RelationTypeResponseModel } from '@umbraco-cms/backend-api';

@customElement('umb-workspace-view-relation-type-relation')
export class UmbWorkspaceViewRelationTypeRelationElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	_RelationType?: RelationTypeResponseModel;

	private _workspaceContext?: UmbRelationTypeWorkspaceContext;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext<UmbRelationTypeWorkspaceContext>('umbWorkspaceContext', (RelationTypeContext) => {
			this._workspaceContext = RelationTypeContext;
			this._observeRelationType();
		});
	}

	private _observeRelationType() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.data, (RelationType) => {
			if (!RelationType) return;
			this._RelationType = RelationType;
		});
	}

	render() {
		return html` ${this._renderGeneralRelation()}${this._renderReferences()} `;
	}

	private _renderGeneralRelation() {
		return html`
			<uui-box headline="General" style="margin-bottom: 20px;">
				<umb-workspace-property-layout label="Key">
					<div slot="relation-typeor">${this._RelationType?.key}</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="Property RelationTypeor Alias">
					<div slot="relation-typeor">${this._RelationType?.propertyRelationTypeorAlias}</div>
				</umb-workspace-property-layout>

				<umb-workspace-property-layout label="Property RelationTypeor UI Alias">
					<div slot="relation-typeor">${this._RelationType?.propertyRelationTypeorUiAlias}</div>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}

	private _renderReferences() {
		return html` <uui-box headline="References"> </uui-box> `;
	}
}

export default UmbWorkspaceViewRelationTypeRelationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-relation-type-relation': UmbWorkspaceViewRelationTypeRelationElement;
	}
}
