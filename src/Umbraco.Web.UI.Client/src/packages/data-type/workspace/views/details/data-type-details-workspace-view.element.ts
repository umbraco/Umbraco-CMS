import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '../../data-type-workspace.context-token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MISSING_PROPERTY_EDITOR_UI_ALIAS } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-data-type-details-workspace-view')
export class UmbDataTypeDetailsWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _propertyEditorUiIcon?: string;

	@state()
	private _propertyEditorUiName?: string;

	@state()
	private _propertyEditorUiAlias?: string;

	@state()
	private _propertyEditorSchemaAlias?: string;

	@state()
	private _propertyEditorDataSourceAlias?: string | null = null;

	@state()
	private _supportsDataSource = false;

	@state()
	private _supportedDataSourceTypes: Array<string> = [];

	#workspaceContext?: typeof UMB_DATA_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeDataType();
		});
	}

	#observeDataType() {
		if (!this.#workspaceContext) {
			return;
		}

		this.observe(this.#workspaceContext.propertyEditorUiAlias, (value) => {
			this._propertyEditorUiAlias = value ?? undefined;
			this.#observePropertyEditorUIManifest();
		});

		this.observe(this.#workspaceContext.propertyEditorSchemaAlias, (value) => {
			this._propertyEditorSchemaAlias = value ?? undefined;
		});

		this.observe(this.#workspaceContext.propertyEditorUiName, (value) => {
			this._propertyEditorUiName = value ?? undefined;
		});

		this.observe(this.#workspaceContext.propertyEditorUiIcon, (value) => {
			this._propertyEditorUiIcon = value ?? undefined;
		});

		this.observe(this.#workspaceContext.propertyEditorDataSourceAlias, (value) => {
			this._propertyEditorDataSourceAlias = value;
		});
	}

	#observePropertyEditorUIManifest() {
		if (!this._propertyEditorUiAlias) return;

		this.observe(umbExtensionsRegistry.byTypeAndAlias('propertyEditorUi', this._propertyEditorUiAlias), (manifest) => {
			this._supportsDataSource = manifest?.meta?.supportsDataSource?.enabled ?? false;
			this._supportedDataSourceTypes = manifest?.meta?.supportsDataSource?.forDataSourceTypes ?? [];
		});
	}

	#onDataSourceChange(event: CustomEvent) {
		const value = (event.target as HTMLInputElement).value;
		this.#workspaceContext?.setPropertyEditorDataSourceAlias(value || undefined);
	}

	override render() {
		return html`
			<uui-box>
				<umb-property-layout
					data-mark="property:editorUiAlias"
					label="Property Editor"
					description=${this.localize.term('propertyEditorPicker_title')}
					mandatory>
					<umb-data-type-details-workspace-property-editor-picker
						slot="editor"
						.propertyEditorUiName=${this._propertyEditorUiName}
						.propertyEditorUiAlias=${this._propertyEditorUiAlias}
						.propertyEditorUiIcon=${this._propertyEditorUiIcon}
						.propertyEditorSchemaAlias=${this._propertyEditorSchemaAlias}
						${umbBindToValidation(this, '$.editorUiAlias', this._propertyEditorUiAlias)}>
					</umb-data-type-details-workspace-property-editor-picker>
				</umb-property-layout>
				${this.#renderDataSourceInput()}
			</uui-box>
			${this.#renderSettings()}
		`;
	}

	#renderDataSourceInput() {
		if (!this._supportsDataSource) return nothing;

		return html`
			<umb-property-layout label="Data Source">
				<umb-input-property-editor-data-source
					.value=${this._propertyEditorDataSourceAlias || ''}
					.dataSourceTypes=${this._supportedDataSourceTypes}
					slot="editor"
					max="1"
					@change=${this.#onDataSourceChange}></umb-input-property-editor-data-source>
			</umb-property-layout>
		`;
	}

	#renderSettings() {
		if (
			!this._propertyEditorUiAlias ||
			!this._propertyEditorUiName ||
			!this._propertyEditorSchemaAlias ||
			this._propertyEditorUiAlias === UMB_MISSING_PROPERTY_EDITOR_UI_ALIAS
		) {
			return nothing;
		}

		return html` <uui-box headline=${this.localize.term('general_settings')}>
			<umb-property-editor-config></umb-property-editor-config>
		</uui-box>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1);
			}

			uui-box {
				margin-top: var(--uui-size-layout-1);
			}

			#btn-add {
				display: block;
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
