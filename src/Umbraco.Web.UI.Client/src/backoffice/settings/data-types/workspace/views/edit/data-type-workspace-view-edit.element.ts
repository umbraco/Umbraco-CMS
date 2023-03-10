import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDataTypeWorkspaceContext } from '../../data-type-workspace.context';
import { UMB_PROPERTY_EDITOR_UI_PICKER_MODAL_TOKEN } from '../../../../../shared/property-editors/modals/property-editor-ui-picker';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DataTypeModel } from '@umbraco-cms/backend-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

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
	_dataType?: DataTypeModel;

	@state()
	private _propertyEditorUIIcon = '';

	@state()
	private _propertyEditorUIName = '';

	@state()
	private _propertyEditorUiAlias = '';

	@state()
	private _propertyEditorAlias = '';

	@state()
	private _data: Array<any> = [];

	private _workspaceContext?: UmbDataTypeWorkspaceContext;
	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		// TODO: Figure out if this is the best way to consume a context or if it could be strongly typed using UmbContextToken
		this.consumeContext<UmbDataTypeWorkspaceContext>('umbWorkspaceContext', (_instance) => {
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
			this._dataType = dataType;

			if (this._dataType.propertyEditorUiAlias !== this._propertyEditorUiAlias) {
				this._observePropertyEditorUI(this._dataType.propertyEditorUiAlias || undefined);
			}

			if (this._dataType.data && this._dataType.data !== this._data) {
				this._data = this._dataType.data;
			}
		});
	}

	private _observePropertyEditorUI(propertyEditorUiAlias?: string) {
		if (!propertyEditorUiAlias) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', propertyEditorUiAlias),
			(propertyEditorUI) => {
				// TODO: show error. We have stored a PropertyEditorUIAlias and can't find the PropertyEditorUI in the registry.
				if (!propertyEditorUI) return;

				this._propertyEditorUIName = propertyEditorUI?.meta.label ?? propertyEditorUI?.name ?? '';
				this._propertyEditorUiAlias = propertyEditorUI?.alias ?? '';
				this._propertyEditorUIIcon = propertyEditorUI?.meta.icon ?? '';
				this._propertyEditorAlias = propertyEditorUI?.meta.propertyEditorModel ?? '';

				this._workspaceContext?.setPropertyEditorAlias(this._propertyEditorAlias);
			}
		);
	}

	private _openPropertyEditorUIPicker() {
		if (!this._dataType) return;

		const modalHandler = this._modalContext?.open(UMB_PROPERTY_EDITOR_UI_PICKER_MODAL_TOKEN, {
			selection: this._propertyEditorUiAlias ? [this._propertyEditorUiAlias] : [],
		});

		modalHandler?.onSubmit().then(({ selection }) => {
			this._selectPropertyEditorUI(selection[0]);
		});
	}

	private _selectPropertyEditorUI(propertyEditorUiAlias: string | undefined) {
		if (!this._dataType || this._dataType.propertyEditorUiAlias === propertyEditorUiAlias) return;
		this._workspaceContext?.setPropertyEditorUiAlias(propertyEditorUiAlias);
		this._observePropertyEditorUI(propertyEditorUiAlias);
	}

	render() {
		return html`
			<uui-box style="margin-bottom: var(--uui-size-space-5);"> ${this._renderPropertyEditorUI()} </uui-box>
			${this._renderConfig()} </uui-box>
		`;
	}

	private _renderPropertyEditorUI() {
		return html`
			<umb-workspace-property-layout label="Property Editor" description="Select a property editor">
				${this._propertyEditorUiAlias
					? html`
							<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
							<umb-ref-property-editor-ui
								slot="editor"
								name=${this._propertyEditorUIName}
								alias=${this._propertyEditorUiAlias}
								property-editor-model-alias=${this._propertyEditorAlias}
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
			${this._propertyEditorAlias && this._propertyEditorUiAlias
				? html`
						<uui-box headline="Config">
							<umb-property-editor-config
								property-editor-ui-alias="${this._propertyEditorUiAlias}"
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
