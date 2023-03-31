import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbDataTypeWorkspaceContext } from './data-type-workspace.context';
import { UmbRouteLocation } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestWorkspace } from '@umbraco-cms/backoffice/extensions-registry';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

/**
 * @element umb-data-type-workspace-edit-element
 * @description - Element for displaying the Data Type Workspace edit route.
 */
@customElement('umb-data-type-workspace-edit-element')
export class UmbDataTypeWorkspaceEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				/* TODO: can this be applied from layout slot CSS? */
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
			}
		`,
	];

	@property()
	manifest?: ManifestWorkspace;

	@property()
	location?: UmbRouteLocation;

	@state()
	private _dataTypeName = '';

	#workspaceContext?: UmbDataTypeWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext as UmbDataTypeWorkspaceContext;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (dataTypeName) => {
			if (dataTypeName !== this._dataTypeName) {
				this._dataTypeName = dataTypeName ?? '';
			}
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.DataType">
				<uui-input slot="header" id="header" .value=${this._dataTypeName} @input="${this.#handleInput}"></uui-input>
			</umb-workspace-layout>
		`;
	}
}

export default UmbDataTypeWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace-edit-element': UmbDataTypeWorkspaceEditElement;
	}
}
