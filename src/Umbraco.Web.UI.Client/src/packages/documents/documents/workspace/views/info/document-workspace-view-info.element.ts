import { UMB_DOCUMENT_PROPERTY_DATASET_CONTEXT, UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../constants.js';
import type { UmbDocumentVariantModel } from '../../../types.js';
import { TimeOptions } from './utils.js';
import { css, customElement, html, ifDefined, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UMB_TEMPLATE_PICKER_MODAL, UmbTemplateItemRepository } from '@umbraco-cms/backoffice/template';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

// import of local components
import './document-workspace-view-info-links.element.js';
import './document-workspace-view-info-history.element.js';
import './document-workspace-view-info-reference.element.js';

@customElement('umb-document-workspace-view-info')
export class UmbDocumentWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _documentUnique = '';

	// Document Type
	@state()
	private _documentTypeUnique?: string = '';

	@state()
	private _documentTypeName?: string;

	@state()
	private _documentTypeIcon?: string;

	@state()
	private _allowedTemplates?: UmbDocumentTypeDetailModel['allowedTemplates'];

	// Template
	@state()
	private _templateUnique = '';

	@state()
	private _templateName?: string;

	@state()
	private _variant?: UmbDocumentVariantModel;

	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	#templateRepository = new UmbTemplateItemRepository(this);

	@state()
	private _routeBuilder?: UmbModalRouteBuilder;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(':entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._routeBuilder = routeBuilder;
			});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this._documentTypeUnique = this.#workspaceContext.getContentTypeId()!;
			this.#observeContent();
		});

		this.consumeContext(UMB_DOCUMENT_PROPERTY_DATASET_CONTEXT, (context) => {
			this.observe(context.currentVariant, (currentVariant) => {
				this._variant = currentVariant;
			});
		});
	}

	#observeContent() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.structure.ownerContentType,
			(documentType) => {
				this._documentTypeName = documentType?.name;
				this._documentTypeIcon = documentType?.icon;
				this._allowedTemplates = documentType?.allowedTemplates;
			},
			'_documentType',
		);

		this.observe(
			this.#workspaceContext.unique,
			(unique) => {
				this._documentUnique = unique!;
			},
			'_documentUnique',
		);

		this.observe(
			this.#workspaceContext.templateId,
			async (templateUnique) => {
				this._templateUnique = templateUnique!;
				if (!templateUnique) return;

				const { data } = await this.#templateRepository.requestItems([templateUnique]);
				if (!data?.length) return;
				this._templateName = data[0].name;
			},
			'_templateUnique',
		);
	}

	#renderStateTag() {
		switch (this._variant?.state) {
			case DocumentVariantStateModel.DRAFT:
				return html`
					<uui-tag look="secondary" label=${this.localize.term('content_unpublished')}>
						${this.localize.term('content_unpublished')}
					</uui-tag>
				`;
			case DocumentVariantStateModel.PUBLISHED:
				return html`
					<uui-tag color="positive" look="primary" label=${this.localize.term('content_published')}>
						${this.localize.term('content_published')}
					</uui-tag>
				`;
			case DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES:
				return html`
					<uui-tag color="positive" look="primary" label=${this.localize.term('content_publishedPendingChanges')}>
						${this.localize.term('content_publishedPendingChanges')}
					</uui-tag>
				`;
			default:
				return html`
					<uui-tag look="primary" label=${this.localize.term('content_notCreated')}>
						${this.localize.term('content_notCreated')}
					</uui-tag>
				`;
		}
	}

	override render() {
		return html`
			<div class="container">
				<umb-document-workspace-view-info-links></umb-document-workspace-view-info-links>
				<umb-document-workspace-view-info-reference
					.documentUnique=${this._documentUnique}></umb-document-workspace-view-info-reference>
				<umb-document-workspace-view-info-history></umb-document-workspace-view-info-history>
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

		return html`
			<div class="general-item">
				<strong><umb-localize key="content_publishStatus">Publication Status</umb-localize></strong>
				<span>${this.#renderStateTag()}</span>
			</div>
			${this.#renderCreateDate()}

			<div class="general-item">
				<strong><umb-localize key="content_documentType">Document Type</umb-localize></strong>
				<uui-ref-node-document-type
					standalone
					href=${editDocumentTypePath + 'edit/' + this._documentTypeUnique}
					name=${ifDefined(this.localize.string(this._documentTypeName ?? ''))}>
					<umb-icon slot="icon" name=${ifDefined(this._documentTypeIcon)}></umb-icon>
				</uui-ref-node-document-type>
			</div>
			${this.#renderTemplateInput()}
			<div class="general-item">
				<strong><umb-localize key="template_id">Id</umb-localize></strong>
				<span>${this._documentUnique}</span>
			</div>
		`;
	}

	#renderTemplateInput() {
		if (this._allowedTemplates?.length === 0) return nothing;

		const editTemplatePath = this._routeBuilder?.({ entityType: 'template' }) ?? '';

		return html`
			<div class="general-item">
				<strong><umb-localize key="template_template">Template</umb-localize></strong>
				${this._templateUnique
					? html`
							<uui-ref-node
								standalone
								name=${ifDefined(this._templateName)}
								href=${editTemplatePath + 'edit/' + this._templateUnique}>
								<uui-icon slot="icon" name="icon-document-html"></uui-icon>
								<uui-action-bar slot="actions">
									<uui-button
										label=${this.localize.term('general_choose')}
										@click=${this.#openTemplatePicker}></uui-button>
								</uui-action-bar>
							</uui-ref-node>
						`
					: html`
							<uui-button
								label=${this.localize.term('general_choose')}
								look="placeholder"
								@click=${this.#openTemplatePicker}></uui-button>
						`}
			</div>
		`;
	}

	#renderCreateDate() {
		if (!this._variant?.createDate) return nothing;

		return html`
			<div class="general-item">
				<strong><umb-localize key="content_createDate">Created</umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._variant.createDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
		`;
	}

	async #openTemplatePicker() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_TEMPLATE_PICKER_MODAL, {
			data: {
				multiple: false,
				pickableFilter: (template) =>
					this._allowedTemplates?.find((allowed) => template.unique === allowed.id) ? true : false,
			},
			value: {
				selection: [this._templateUnique],
			},
		});

		const result = await modal?.onSubmit().catch(() => undefined);

		if (!result?.selection.length) return;

		const templateUnique = result.selection[0];

		if (!templateUnique) return;

		this.#workspaceContext?.setTemplate(templateUnique);
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

			//General section

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

			.variant-state {
				display: flex;
				gap: var(--uui-size-space-4);
			}

			.variant-state > span {
				color: var(--uui-color-divider-emphasis);
			}
		`,
	];
}

export default UmbDocumentWorkspaceViewInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-info': UmbDocumentWorkspaceViewInfoElement;
	}
}
