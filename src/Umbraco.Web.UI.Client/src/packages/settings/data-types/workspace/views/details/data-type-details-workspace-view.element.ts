import { UMB_DATA_TYPE_WORKSPACE_CONTEXT, UmbDataTypeWorkspaceContext } from '../../data-type-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_PROPERTY_EDITOR_UI_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	UmbWorkspaceEditorViewExtensionElement,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-data-type-details-workspace-view')
export class UmbDataTypeDetailsWorkspaceViewEditElement
	extends UmbLitElement
	implements UmbWorkspaceEditorViewExtensionElement
{
	@state()
	_dataType?: DataTypeResponseModel;

	@state()
	private _propertyEditorUiIcon?: string;

	@state()
	private _propertyEditorUiName?: string;

	@state()
	private _propertyEditorUiAlias?: string;

	@state()
	private _propertyEditorSchemaAlias?: string;

	@state()
	private _data: Array<any> = [];

	private _workspaceContext?: UmbDataTypeWorkspaceContext;
	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (_instance) => {
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

			if (!this._dataType.propertyEditorUiAlias) {
				if (this._dataType.propertyEditorAlias) {
					// Get the property editor UI alias from the property editor alias:
					this.observe(
						umbExtensionsRegistry.getByTypeAndAlias('propertyEditorSchema', this._dataType.propertyEditorAlias),
						(propertyEditorSchema) => {
							// TODO: show error. We have stored a propertyEditorSchemaAlias and can't find the PropertyEditorSchema in the registry.
							if (!propertyEditorSchema) return;
							this._setPropertyEditorUiAlias(propertyEditorSchema.meta.defaultPropertyEditorUiAlias ?? undefined);
						},
						'_observepropertyEditorSchemaForDefaultUI'
					);
				} else {
					this._setPropertyEditorUiAlias(undefined);
				}
			} else {
				this._setPropertyEditorUiAlias(this._dataType.propertyEditorUiAlias);
			}

			if (this._dataType.values && this._dataType.values !== this._data) {
				this._data = this._dataType.values;
			}
		});
	}

	private _setPropertyEditorUiAlias(value: string | undefined) {
		const oldValue = this._propertyEditorUiAlias;
		if (oldValue !== value) {
			this._propertyEditorUiAlias = value;
			this._observePropertyEditorUI(value || undefined);
		}
	}

	private _observePropertyEditorUI(propertyEditorUiAlias?: string) {
		if (!propertyEditorUiAlias) {
			this._propertyEditorUiName = this._propertyEditorUiIcon = this._propertyEditorUiAlias = undefined;
			this.removeControllerByAlias('_observePropertyEditorUI');
			return;
		}

		// remove the '_observepropertyEditorSchemaForDefaultUI' controller, as we do not want to observe for default value anymore:
		this.removeControllerByAlias('_observepropertyEditorSchemaForDefaultUI');

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUi', propertyEditorUiAlias),
			(propertyEditorUI) => {
				// TODO: show error. We have stored a PropertyEditorUIAlias and can't find the PropertyEditorUI in the registry.
				if (!propertyEditorUI) return;

				this._propertyEditorUiName = propertyEditorUI?.meta.label ?? propertyEditorUI?.name ?? '';
				this._propertyEditorUiAlias = propertyEditorUI?.alias ?? '';
				this._propertyEditorUiIcon = propertyEditorUI?.meta.icon ?? '';
				this._propertyEditorSchemaAlias = propertyEditorUI?.meta.propertyEditorSchemaAlias ?? '';

				this._workspaceContext?.setPropertyEditorSchemaAlias(this._propertyEditorSchemaAlias);
			},
			'_observePropertyEditorUI'
		);
	}

	private _openPropertyEditorUIPicker() {
		if (!this._dataType) return;

		const modalContext = this._modalContext?.open(UMB_PROPERTY_EDITOR_UI_PICKER_MODAL, {
			selection: this._propertyEditorUiAlias ? [this._propertyEditorUiAlias] : [],
		});

		modalContext?.onSubmit().then(({ selection }) => {
			this._selectPropertyEditorUI(selection[0]);
		});
	}

	private _selectPropertyEditorUI(propertyEditorUiAlias: string | undefined) {
		this._workspaceContext?.setPropertyEditorUiAlias(propertyEditorUiAlias);
	}

	render() {
		return html`
			<uui-box> ${this._renderPropertyEditorUI()} </uui-box>
			${this._renderConfig()} </uui-box>
		`;
	}

	private _renderPropertyEditorUI() {
		return html`
			<umb-workspace-property-layout label="Property Editor" description="Select a property editor">
				${this._propertyEditorUiAlias && this._propertyEditorSchemaAlias
					? html`
							<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
							<umb-ref-property-editor-ui
								slot="editor"
								name=${this._propertyEditorUiName ?? ''}
								alias=${this._propertyEditorUiAlias}
								property-editor-schema-alias=${this._propertyEditorSchemaAlias}
								border>
								${this._propertyEditorUiIcon
									? html` <uui-icon name="${this._propertyEditorUiIcon}" slot="icon"></uui-icon> `
									: ''}
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
			${this._propertyEditorSchemaAlias && this._propertyEditorUiAlias
				? html`
						<uui-box headline="Settings">
							<umb-property-editor-config
								property-editor-ui-alias="${this._propertyEditorUiAlias}"
								.data="${this._data}"></umb-property-editor-config>
						</uui-box>
				  `
				: nothing}
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1);
			}

			uui-box {
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbDataTypeDetailsWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-details-workspace-view': UmbDataTypeDetailsWorkspaceViewEditElement;
	}
}
