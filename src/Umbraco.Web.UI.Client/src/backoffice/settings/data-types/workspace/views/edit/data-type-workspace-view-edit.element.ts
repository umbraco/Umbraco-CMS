import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../../../core/modal';
import { UmbWorkspaceDataTypeContext } from '../../data-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/context-api';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../../../../../shared/property-editors/shared/property-editor-config/property-editor-config.element';
import '../../../../../shared/components/ref-property-editor-ui/ref-property-editor-ui.element';

@customElement('umb-data-type-workspace-view-edit')
export class UmbDataTypeWorkspaceViewEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];

	@state()
	_dataType?: DataTypeDetails;

	@state()
	private _propertyEditorUIIcon = '';

	@state()
	private _propertyEditorUIName = '';

	@state()
	private _propertyEditorUIAlias = '';

	@state()
	private _propertyEditorModelAlias = '';

	@state()
	private _data: Array<any> = [];

	private _workspaceContext?: UmbWorkspaceDataTypeContext;
	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (_instance) => {
			this._modalService = _instance;
		});

		// TODO: Figure out if this is the best way to consume a context or if it could be strongly typed using UmbContextToken
		this.consumeContext<UmbWorkspaceDataTypeContext>('umbWorkspaceContext', (_instance) => {
			this._workspaceContext = _instance;
			this._observeDataType();
		});
	}

	private _observeDataType() {
		if (!this._workspaceContext) {
			return;
		}

		this.observe(this._workspaceContext.data, (dataType) => {
			if (!dataType) return;

			// TODO: handle if model is not of the type wanted.
			this._dataType = dataType as DataTypeDetails;

			if (this._dataType.propertyEditorUIAlias !== this._propertyEditorUIAlias) {
				this._observePropertyEditorUI(this._dataType.propertyEditorUIAlias);
			}

			if (this._dataType.data !== this._data) {
				this._data = this._dataType.data;
			}
		});
	}

	private _observePropertyEditorUI(propertyEditorUIAlias: string | null) {
		if (!propertyEditorUIAlias) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', propertyEditorUIAlias),
			(propertyEditorUI) => {
				// TODO: show error. We have stored a PropertyEditorUIAlias and can't find the PropertyEditorUI in the registry.
				if (!propertyEditorUI) return;

				this._propertyEditorUIName = propertyEditorUI?.meta.label ?? propertyEditorUI?.name ?? '';
				this._propertyEditorUIAlias = propertyEditorUI?.alias ?? '';
				this._propertyEditorUIIcon = propertyEditorUI?.meta.icon ?? '';
				this._propertyEditorModelAlias = propertyEditorUI?.meta.propertyEditorModel ?? '';

				this._workspaceContext?.update({ propertyEditorModelAlias: this._propertyEditorModelAlias });
			}
		);
	}

	private _openPropertyEditorUIPicker() {
		if (!this._dataType) return;

		const modalHandler = this._modalService?.propertyEditorUIPicker({
			selection: this._propertyEditorUIAlias ? [this._propertyEditorUIAlias] : [],
		});

		modalHandler?.onClose().then(({ selection } = {}) => {
			if (!selection) return;
			this._selectPropertyEditorUI(selection[0]);
		});
	}

	private _selectPropertyEditorUI(propertyEditorUIAlias: string | null) {
		if (!this._dataType || this._dataType.propertyEditorUIAlias === propertyEditorUIAlias) return;
		this._workspaceContext?.update({ propertyEditorUIAlias });
		this._observePropertyEditorUI(propertyEditorUIAlias);
	}

	render() {
		return html`
			<uui-box style="margin-bottom: 20px;"> ${this._renderPropertyEditorUI()} </uui-box>
			${this._renderConfig()} </uui-box>
		`;
	}

	private _renderPropertyEditorUI() {
		return html`
			<umb-workspace-property-layout label="Property Editor" description="Select a property editor">
				${this._propertyEditorUIAlias
					? html`
							<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
							<umb-ref-property-editor-ui
								slot="editor"
								name=${this._propertyEditorUIName}
								alias=${this._propertyEditorUIAlias}
								property-editor-model-alias=${this._propertyEditorModelAlias}
								border>
								<uui-icon name="${this._propertyEditorUIIcon}" slot="icon"></uui-icon>
								<uui-action-bar slot="actions">
									<uui-button label="Change" @click=${this._openPropertyEditorUIPicker}></uui-button>
								</uui-action-bar>
							</umb-ref-property-editor-ui>
					  `
					: html`
							<uui-button
								slot="editor"
								label="Select Property Editor"
								look="placeholder"
								color="default"
								@click=${this._openPropertyEditorUIPicker}></uui-button>
					  `}
			</umb-workspace-property-layout>
		`;
	}

	private _renderConfig() {
		return html`
			${this._propertyEditorModelAlias && this._propertyEditorUIAlias
				? html`
						<uui-box headline="Config">
							<umb-property-editor-config
								property-editor-ui-alias="${this._propertyEditorUIAlias}"
								.data="${this._data}"></umb-property-editor-config>
						</uui-box>
				  `
				: nothing}
		`;
	}
}

export default UmbDataTypeWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace-view-edit': UmbDataTypeWorkspaceViewEditElement;
	}
}
