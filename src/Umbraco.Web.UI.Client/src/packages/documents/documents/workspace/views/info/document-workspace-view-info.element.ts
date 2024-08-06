import { TimeOptions } from './utils.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import { UMB_TEMPLATE_PICKER_MODAL, UmbTemplateItemRepository } from '@umbraco-cms/backoffice/template';
import type { DocumentUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbDocumentVariantModel } from '@umbraco-cms/backoffice/document';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

// import of local components
import './document-workspace-view-info-history.element.js';
import './document-workspace-view-info-reference.element.js';

@customElement('umb-document-workspace-view-info')
export class UmbDocumentWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _invariantCulture = 'en-US';

	@state()
	private _documentUnique = '';

	@state()
	private _urls?: Array<DocumentUrlInfoModel>;

	@state()
	private _createDate?: string;

	/**Document Type */
	@state()
	private _documentTypeUnique = '';

	@state()
	private _documentTypeName?: string;

	@state()
	private _documentTypeIcon?: string;

	@state()
	private _allowedTemplates?: UmbDocumentTypeDetailModel['allowedTemplates'];

	/**Template */
	@state()
	private _templateUnique = '';

	@state()
	private _templateName?: string;

	@state()
	private _variants: UmbDocumentVariantModel[] = [];

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
			this.#workspaceContext.urls,
			(urls) => {
				this._urls = urls;
			},
			'_documentUrls',
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

		this.observe(
			this.#workspaceContext.variants,
			(variants) => {
				this._variants = variants;
				this.#observeVariants();
			},
			'_variants',
		);
	}

	#observeVariants() {
		// Find the oldest variant
		const oldestVariant =
			this._variants.length > 0
				? this._variants
						.filter((v) => !!v.createDate)
						.reduce((prev, current) => (prev.createDate! < current.createDate! ? prev : current))
				: null;

		this._createDate = oldestVariant?.createDate ?? new Date().toISOString();
	}

	#renderVariantStates() {
		return repeat(
			this._variants,
			(variant) => `${variant.culture}_${variant.segment}`,
			(variant) => html`
				<div class="variant-state">
					<span>${variant.culture ?? this._invariantCulture}</span>
					${this.#renderStateTag(variant)}
				</div>
			`,
		);
	}

	#renderStateTag(variant: UmbDocumentVariantModel) {
		switch (variant.state) {
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
						${this.localize.term('content_published')}
					</uui-tag>
				`;
			default:
				return html`
					<uui-tag look="primary" label=${this.localize.term('content_published')}>
						${this.localize.term('content_published')}
					</uui-tag>
				`;
		}
	}

	override render() {
		return html`
			<div class="container">
				<uui-box headline=${this.localize.term('general_links')} style="--uui-box-default-padding: 0;">
					<div id="link-section">${this.#renderLinksSection()}</div>
				</uui-box>

				<umb-document-workspace-view-info-reference
					.documentUnique=${this._documentUnique}></umb-document-workspace-view-info-reference>

				<umb-document-workspace-view-info-history
					.documentUnique=${this._documentUnique}></umb-document-workspace-view-info-history>
			</div>
			<div class="container">
				<uui-box headline=${this.localize.term('general_general')} id="general-section"
					>${this.#renderGeneralSection()}</uui-box
				>
			</div>
		`;
	}

	#renderLinksSection() {
		/** TODO Make sure link section is completed */
		if (this._urls && this._urls.length) {
			return html`
				${repeat(
					this._urls,
					(url) => url.culture,
					(url) => html`
						<a href=${url.url} target="_blank" class="link-item with-href">
							<span class="link-language">${url.culture}</span>
							<span class="link-content"> ${url.url}</span>
							<uui-icon name="icon-out"></uui-icon>
						</a>
					`,
				)}
			`;
		} else {
			return html`
				<div class="link-item">
					<span class="link-language">${this._invariantCulture}</span>
					<span class="link-content italic"><umb-localize key="content_parentNotPublishedAnomaly"></umb-localize></span>
				</div>
			`;
		}
	}

	#renderGeneralSection() {
		const editDocumentTypePath = this._routeBuilder?.({ entityType: 'document-type' }) ?? '';
		const editTemplatePath = this._routeBuilder?.({ entityType: 'template' }) ?? '';

		return html`
			<div class="general-item">
				<strong><umb-localize key="content_publishStatus">Publication Status</umb-localize></strong>
				<umb-stack look="compact">${this.#renderVariantStates()}</umb-stack>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_createDate">Created</umb-localize></strong>
				<span>
					<umb-localize-date .date=${this._createDate} .options=${TimeOptions}></umb-localize-date>
				</span>
			</div>
			<div class="general-item">
				<strong><umb-localize key="content_documentType">Document Type</umb-localize></strong>
				<uui-ref-node-document-type
					standalone
					href=${editDocumentTypePath + 'edit/' + this._documentTypeUnique}
					name=${ifDefined(this.localize.string(this._documentTypeName ?? ''))}>
					<umb-icon slot="icon" name=${ifDefined(this._documentTypeIcon)}></umb-icon>
				</uui-ref-node-document-type>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_template">Template</umb-localize></strong>
				${this._templateUnique
					? html`
							<uui-ref-node
								standalone
								name=${ifDefined(this._templateName)}
								href=${editTemplatePath + 'edit/' + this._templateUnique}>
								<uui-icon slot="icon" name="icon-newspaper"></uui-icon>
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
			<div class="general-item">
				<strong><umb-localize key="template_id">Id</umb-localize></strong>
				<span>${this._documentUnique}</span>
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
		UmbTextStyles,
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

			// Link section

			#link-section {
				display: flex;
				flex-direction: column;
				text-align: left;
			}

			.link-item {
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				display: grid;
				grid-template-columns: auto 1fr auto;
				gap: var(--uui-size-6);
				color: inherit;
				text-decoration: none;
			}

			.link-language {
				color: var(--uui-color-divider-emphasis);
			}

			.link-content.italic {
				font-style: italic;
			}

			.link-item uui-icon {
				margin-right: var(--uui-size-space-2);
				vertical-align: middle;
			}

			.link-item.with-href {
				cursor: pointer;
			}

			.link-item.with-href:hover {
				background: var(--uui-color-divider);
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
