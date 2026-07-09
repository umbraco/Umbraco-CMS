import type { UmbDocumentBlueprintVariantModel } from '../../../types.js';
import { UMB_DOCUMENT_BLUEPRINT_PROPERTY_DATASET_CONTEXT } from '../../../property-dataset-context/document-blueprint-property-dataset-context.token.js';
import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT } from '../../constants.js';
import { css, customElement, html, ifDefined, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_DATE_TIME_FORMAT_OPTIONS } from '@umbraco-cms/backoffice/utils';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-document-blueprint-workspace-view-info')
export class UmbDocumentBlueprintWorkspaceViewInfoElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _documentBlueprintUnique = '';

	@state()
	private _routeBuilder?: UmbModalRouteBuilder;

	@state()
	private _documentTypeUnique?: string = '';

	@state()
	private _documentTypeName?: string;

	@state()
	private _documentTypeIcon?: string;

	@state()
	private _hasSettingsAccess: boolean = false;

	@state()
	private _variant?: UmbDocumentBlueprintVariantModel;

	#workspaceContext?: typeof UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('general/:entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._routeBuilder = routeBuilder;
			});

		this.consumeContext(UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this._documentTypeUnique = this.#workspaceContext?.getContentTypeUnique();
			this._documentBlueprintUnique = this.#workspaceContext?.getUnique() ?? '';
			this.#observeContent();
		});

		this.consumeContext(UMB_DOCUMENT_BLUEPRINT_PROPERTY_DATASET_CONTEXT, (context) => {
			this.observe(context?.currentVariant, (currentVariant) => {
				this._variant = currentVariant;
			});
		});

		createExtensionApiByAlias(this, UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					match: UMB_SETTINGS_SECTION_ALIAS,
				},
				onChange: (permitted: boolean) => {
					this._hasSettingsAccess = permitted;
				},
			},
		]);
	}

	#observeContent() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.structure.ownerContentType,
			(documentType) => {
				this._documentTypeName = documentType?.name;
				this._documentTypeIcon = documentType?.icon;
			},
			'_documentType',
		);

		this.observe(
			this.#workspaceContext.unique,
			(unique) => {
				this._documentBlueprintUnique = unique!;
			},
			'_documentBlueprintUnique',
		);
	}

	override render() {
		return html`
			<div class="container">
				<umb-extension-slot id="workspace-info-apps" type="workspaceInfoApp"></umb-extension-slot>
			</div>
			<div class="container">
				<uui-box headline=${this.localize.term('general_general')} id="general-section">
					${this.#renderGeneralSection()}
				</uui-box>
			</div>
		`;
	}

	#renderGeneralSection() {
		const editDocumentTypePath = this._routeBuilder?.({ entityType: 'document-type' }) ?? '';

		return html`${this.#renderCreateDate()} ${this.#renderUpdateDate()}

			<div class="general-item">
				<strong><umb-localize key="content_documentType">Document Type</umb-localize></strong>
				<uui-ref-node-document-type
					standalone
					href=${ifDefined(
						this._hasSettingsAccess ? editDocumentTypePath + 'edit/' + this._documentTypeUnique : undefined,
					)}
					?readonly=${!this._hasSettingsAccess}
					name=${ifDefined(this.localize.string(this._documentTypeName ?? ''))}>
					<umb-icon slot="icon" name=${ifDefined(this._documentTypeIcon)}></umb-icon>
				</uui-ref-node-document-type>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_id">Id</umb-localize></strong>
				<span>${this._documentBlueprintUnique}</span>
			</div>`;
	}

	#renderCreateDate() {
		if (!this._variant?.createDate) return nothing;
		return this.#renderDate(this._variant.createDate, 'content_createDate', 'Created');
	}

	#renderUpdateDate() {
		if (!this._variant?.updateDate) return nothing;
		return this.#renderDate(this._variant.updateDate, 'content_updateDate', 'Last edited');
	}

	#renderDate(date: string, labelKey: string, labelText: string) {
		return html`
			<div class="general-item">
				<strong><umb-localize .key=${labelKey}>${labelText}</umb-localize></strong>
				<span>
					<umb-localize-date .date=${date} .options=${UMB_DATE_TIME_FORMAT_OPTIONS}></umb-localize-date>
				</span>
			</div>
		`;
	}
	static override styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
				grid-template-columns: 1fr 350px;
			}

			div.container {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
			}

			#general-section {
				display: flex;
				flex-direction: column;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
			}

			uui-ref-node-document-type[readonly] {
				padding-top: 7px;
				padding-bottom: 7px;
			}
		`,
	];
}

export default UmbDocumentBlueprintWorkspaceViewInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-workspace-view-info': UmbDocumentBlueprintWorkspaceViewInfoElement;
	}
}
