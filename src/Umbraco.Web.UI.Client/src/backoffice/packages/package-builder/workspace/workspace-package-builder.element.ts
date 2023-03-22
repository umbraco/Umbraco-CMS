import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UUIBooleanInputEvent, UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { css, html, nothing } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbInputDocumentPickerElement } from '../../../shared/components/input-document-picker/input-document-picker.element';
import { UmbInputMediaPickerElement } from '../../../shared/components/input-media-picker/input-media-picker.element';
import { UmbInputLanguagePickerElement } from '../../../shared/components/input-language-picker/input-language-picker.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PackageDefinitionResponseModel, PackageResource } from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

@customElement('umb-workspace-package-builder')
export class UmbWorkspacePackageBuilderElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			.header {
				margin: 0 var(--uui-size-layout-1);
				display: flex;
				gap: var(--uui-size-space-4);
			}

			uui-box {
				margin: var(--uui-size-layout-1);
			}

			uui-checkbox {
				margin-top: var(--uui-size-space-4);
			}
		`,
	];

	@property()
	entityKey?: string;

	@state()
	private _package: PackageDefinitionResponseModel = {};

	@query('#package-name-input')
	private _packageNameInput!: UUIInputElement;

	private _notificationContext?: UmbNotificationContext;

	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this._notificationContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		if (this.entityKey) this.#getPackageCreated();
	}

	async #getPackageCreated() {
		if (!this.entityKey) return;
		const { data } = await tryExecuteAndNotify(this, PackageResource.getPackageCreatedByKey({ key: this.entityKey }));
		if (!data) return;
		this._package = data as PackageDefinitionResponseModel;
	}

	async #download() {
		if (!this._package?.key) return;
		const response = await tryExecuteAndNotify(
			this,
			PackageResource.getPackageCreatedByKeyDownload({ key: this._package.key })
		);
	}

	#nameDefined() {
		const valid = this._packageNameInput.checkValidity();
		if (!valid) this._notificationContext?.peek('danger', { data: { message: 'Package missing a name' } });
		return valid;
	}

	async #save() {
		if (!this.#nameDefined()) return;
		const response = await tryExecuteAndNotify(
			this,
			PackageResource.postPackageCreated({ requestBody: this._package })
		);
		if (!response.data || response.error) return;
		this._package = response.data as PackageDefinitionResponseModel;
		this.#navigateBack();
	}

	async #update() {
		if (!this.#nameDefined()) return;
		if (!this._package?.key) return;
		const response = await tryExecuteAndNotify(
			this,
			PackageResource.putPackageCreatedByKey({ key: this._package.key, requestBody: this._package })
		);

		if (response.error) return;
		this.#navigateBack();
	}

	#navigateBack() {
		window.history.pushState({}, '', '/section/packages/view/created');
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.PackageBuilder">
				${this.#renderHeader()}
				<uui-box class="wrapper" headline="Package Content"> ${this.#renderEditors()} </uui-box>
				${this.#renderActions()}
			</umb-workspace-layout>
		`;
	}

	#renderHeader() {
		return html`<div class="header" slot="header">
			<uui-button compact @click="${this.#navigateBack}" label="Back to created package overview">
				<uui-icon name="umb:arrow-left"></uui-icon>
			</uui-button>
			<uui-input
				required
				id="package-name-input"
				label="Name of the package"
				placeholder="Enter a name"
				value="${ifDefined(this._package?.name)}"
				@change="${(e: UUIInputEvent) => (this._package.name = e.target.value as string)}"></uui-input>
		</div>`;
	}

	#renderActions() {
		return html`<div slot="actions">
			${this._package?.key
				? html`<uui-button @click="${this.#download}" color="" look="secondary" label="Download package">
						Download
				  </uui-button>`
				: nothing}
			<uui-button
				@click="${this._package.key ? this.#update : this.#save}"
				color="positive"
				look="primary"
				label="Save changes to package">
				Save
			</uui-button>
		</div>`;
	}

	#renderEditors() {
		return html`<umb-workspace-property-layout label="Content" description="">
				${this.#renderContentSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Media" description=""
				>${this.#renderMediaSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Document Types" description="">
				${this.#renderDocumentTypeSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Media Types" description="">
				${this.#renderMediaTypeSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Languages" description="">
				${this.#renderLanguageSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Dictionary" description="">
				${this.#renderDictionarySection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Data Types" description="">
				${this.#renderDataTypeSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Templates" description="">
				${this.#renderTemplateSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Stylesheets" description="">
				${this.#renderStylesheetsSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Scripts" description="">
				${this.#renderScriptsSection()}
			</umb-workspace-property-layout>

			<umb-workspace-property-layout label="Partial Views" description="">
				${this.#renderPartialViewSection()}
			</umb-workspace-property-layout>`;
	}

	#renderContentSection() {
		return html`
			<div slot="editor">
				<umb-input-document-picker
					.value=${this._package.contentNodeId ?? ''}
					max="1"
					@change="${(e: CustomEvent) =>
						(this._package.contentNodeId = (e.target as UmbInputDocumentPickerElement).selectedKeys[0])}">
				</umb-input-document-picker>
				<uui-checkbox
					label="Include child nodes"
					.checked="${this._package.contentLoadChildNodes ?? false}"
					@change="${(e: UUIBooleanInputEvent) => (this._package.contentLoadChildNodes = e.target.checked)}">
					Include child nodes
				</uui-checkbox>
			</div>
		`;
	}

	#renderMediaSection() {
		return html`
			<div slot="editor">
				<umb-input-media-picker
					.selectedKeys=${this._package.mediaKeys ?? []}
					@change="${(e: CustomEvent) =>
						(this._package.mediaKeys = (
							e.target as UmbInputMediaPickerElement
						).selectedKeys)}"></umb-input-media-picker>
				<uui-checkbox
					label="Include child nodes"
					.checked="${this._package.mediaLoadChildNodes ?? false}"
					@change="${(e: UUIBooleanInputEvent) => (this._package.mediaLoadChildNodes = e.target.checked)}">
					Include child nodes
				</uui-checkbox>
			</div>
		`;
	}

	#renderDocumentTypeSection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}

	#renderMediaTypeSection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}

	#renderLanguageSection() {
		return html`<div slot="editor">
			<umb-input-language-picker
				.value="${this._package.languages?.join(',') ?? ''}"
				@change="${(e: CustomEvent) => {
					this._package.languages = (e.target as UmbInputLanguagePickerElement).selectedIsoCodes;
				}}"></umb-input-language-picker>
		</div>`;
	}

	#renderDictionarySection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}

	#renderDataTypeSection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}

	#renderTemplateSection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}

	#renderStylesheetsSection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}

	#renderScriptsSection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}

	#renderPartialViewSection() {
		return html`<div slot="editor">
			<umb-input-checkbox-list></umb-input-checkbox-list>
		</div>`;
	}
}

export default UmbWorkspacePackageBuilderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-package-builder': UmbWorkspacePackageBuilderElement;
	}
}
