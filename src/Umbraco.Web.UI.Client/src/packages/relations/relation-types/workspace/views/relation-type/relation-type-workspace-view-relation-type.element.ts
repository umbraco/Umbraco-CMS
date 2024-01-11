import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from '../../relation-type-workspace.context.js';
import {
	UUIBooleanInputEvent,
	UUIRadioGroupElement,
	UUIRadioGroupEvent,
	UUISelectEvent,
	UUIToggleElement,
} from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { RelationTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-relation-type-workspace-view-relation-type')
export class UmbRelationTypeWorkspaceViewRelationTypeElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _relationType?: RelationTypeResponseModel;

	#workspaceContext?: typeof UMB_RELATION_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_RELATION_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this._observeRelationType();
		});
	}

	private _observeRelationType() {
		if (!this.#workspaceContext) {
			return;
		}

		this.observe(this.#workspaceContext.data, (relationType) => {
			if (!relationType) return;

			this._relationType = relationType;
		});
	}

	#handleDirectionChange(event: UUIRadioGroupEvent) {
		const target = event.target as UUIRadioGroupElement;
		const value = target.value === 'true';
		this.#workspaceContext?.update('isBidirectional', value);
	}

	#handleIsDependencyChange(event: UUIBooleanInputEvent) {
		const target = event.target as UUIToggleElement;
		const value = target.checked;
		this.#workspaceContext?.update('isDependency', value);
	}

	render() {
		return html`
			<uui-box>
				<umb-property-layout label="Direction">
					<uui-radio-group
						value=${ifDefined(this._relationType?.isBidirectional)}
						@change=${this.#handleDirectionChange}
						slot="editor">
						<uui-radio label="Parent to child" value="false"></uui-radio>
						<uui-radio label="Bidirectional" value="true"></uui-radio>
					</uui-radio-group>
				</umb-property-layout>
				<umb-property-layout label="Parent">${this.#renderParentProperty()}</umb-property-layout>
				<umb-property-layout label="Child"> ${this.#renderChildProperty()} </umb-property-layout>
				<umb-property-layout label="Is dependency">
					<uui-toggle
						slot="editor"
						@change=${this.#handleIsDependencyChange}
						.checked=${this._relationType?.isDependency ?? false}></uui-toggle>
				</umb-property-layout>
			</uui-box>
		`;
	}

	#onParentObjectTypeChange(event: UUISelectEvent) {
		const value = event.target.value as string;
		this.#workspaceContext?.update('parentObjectType', value);
	}
	#onChildObjectTypeChange(event: UUISelectEvent) {
		const value = event.target.value as string;
		this.#workspaceContext?.update('childObjectType', value);
	}

	#renderParentProperty() {
		if (!this.#workspaceContext?.getIsNew() && this._relationType)
			return html`<div slot="editor">${this._relationType.parentObjectTypeName}</div>`;

		return html`
			<umb-input-object-type @change=${this.#onParentObjectTypeChange} slot="editor"></umb-input-object-type>
		`;
	}

	#renderChildProperty() {
		if (!this.#workspaceContext?.getIsNew() && this._relationType)
			return html`<div slot="editor">${this._relationType.childObjectTypeName}</div>`;

		return html`
			<umb-input-object-type @change=${this.#onChildObjectTypeChange} slot="editor"></umb-input-object-type>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbRelationTypeWorkspaceViewRelationTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-type-workspace-view-relation-type': UmbRelationTypeWorkspaceViewRelationTypeElement;
	}
}
