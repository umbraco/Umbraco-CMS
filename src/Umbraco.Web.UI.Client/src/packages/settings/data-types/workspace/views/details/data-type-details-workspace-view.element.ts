import { UmbDataTypeWorkspaceContext } from '../../data-type-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
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
	private _propertyEditorAlias?: string;

	@state()
	private _data: Array<any> = [];

	private _workspaceContext?: UmbDataTypeWorkspaceContext;
	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		// TODO: Figure out if this is the best way to consume a context or if it could be strongly typed using UmbContextToken
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (_instance) => {
			this._workspaceContext = _instance as UmbDataTypeWorkspaceContext;
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
						umbExtensionsRegistry.getByTypeAndAlias('propertyEditorModel', this._dataType.propertyEditorAlias),
						(propertyEditorModel) => {
							// TODO: show error. We have stored a PropertyEditorModelAlias and can't find the PropertyEditorModel in the registry.
							if (!propertyEditorModel) return;
							this._setPropertyEditorUiAlias(propertyEditorModel.meta.defaultPropertyEditorUiAlias ?? undefined);
						},
						'_observePropertyEditorModelForDefaultUI'
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
			this.removeControllerByUnique('_observePropertyEditorUI');
			return;
		}

		// remove the '_observePropertyEditorModelForDefaultUI' controller, as we do not want to observe for default value anymore:
		this.removeControllerByUnique('_observePropertyEditorModelForDefaultUI');

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUi', propertyEditorUiAlias),
			(propertyEditorUI) => {
				// TODO: show error. We have stored a PropertyEditorUIAlias and can't find the PropertyEditorUI in the registry.
				if (!propertyEditorUI) return;

				this._propertyEditorUiName = propertyEditorUI?.meta.label ?? propertyEditorUI?.name ?? '';
				this._propertyEditorUiAlias = propertyEditorUI?.alias ?? '';
				this._propertyEditorUiIcon = propertyEditorUI?.meta.icon ?? '';
				this._propertyEditorAlias = propertyEditorUI?.meta.propertyEditorAlias ?? '';

				this._workspaceContext?.setPropertyEditorAlias(this._propertyEditorAlias);
			},
			'_observePropertyEditorUI'
		);
	}

	private _openPropertyEditorUIPicker() {
		if (!this._dataType) return;

		const modalHandler = this._modalContext?.open(UMB_PROPERTY_EDITOR_UI_PICKER_MODAL, {
			selection: this._propertyEditorUiAlias ? [this._propertyEditorUiAlias] : [],
		});

		modalHandler?.onSubmit().then(({ selection }) => {
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
				${this._propertyEditorUiAlias
					? html`
							<!-- TODO: border is a bit weird attribute name. Maybe single or standalone would be better? -->
							<umb-ref-property-editor-ui
								slot="editor"
								name=${this._propertyEditorUiName}
								alias=${this._propertyEditorUiAlias}
								property-editor-model-alias=${this._propertyEditorAlias}
								border>
								<uui-icon name="${this._propertyEditorUiIcon}" slot="icon"></uui-icon>
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
				padding: var(--uui-size-layout-1);
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
