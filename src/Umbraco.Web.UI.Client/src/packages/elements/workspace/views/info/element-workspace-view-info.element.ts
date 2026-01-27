import { UMB_ELEMENT_WORKSPACE_CONTEXT } from '../../constants.js';
import { UMB_ELEMENT_WORKSPACE_PROPERTY_DATASET_CONTEXT } from '../../property-dataset-context/element-workspace-property-dataset-context.token.js';
import type { UmbElementVariantModel } from '../../../types.js';
import { UmbElementVariantState } from '../../../types.js';
import { css, customElement, html, ifDefined, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

const TimeOptions: Intl.DateTimeFormatOptions = {
	year: 'numeric',
	month: 'long',
	day: 'numeric',
	hour: 'numeric',
	minute: 'numeric',
	second: 'numeric',
};

@customElement('umb-element-workspace-view-info')
export class UmbElementWorkspaceViewInfoElement extends UmbLitElement {
	@state()
	private _elementUnique = '';

	// Element Type (Document Type)
	@state()
	private _elementTypeUnique?: string = '';

	@state()
	private _elementTypeName?: string;

	@state()
	private _elementTypeIcon?: string;

	@state()
	private _variant?: UmbElementVariantModel;

	@state()
	private _hasSettingsAccess: boolean = false;

	#workspaceContext?: typeof UMB_ELEMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _routeBuilder?: UmbModalRouteBuilder;

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

		this.consumeContext(UMB_ELEMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this._elementTypeUnique = this.#workspaceContext?.getContentTypeUnique();
			this.#observeContent();
		});

		this.consumeContext(UMB_ELEMENT_WORKSPACE_PROPERTY_DATASET_CONTEXT, (context) => {
			this.observe(context?.currentVariant, (currentVariant) => {
				this._variant = currentVariant as UmbElementVariantModel | undefined;
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
			(elementType) => {
				this._elementTypeName = (elementType as UmbDocumentTypeDetailModel | undefined)?.name;
				this._elementTypeIcon = (elementType as UmbDocumentTypeDetailModel | undefined)?.icon;
			},
			'_elementType',
		);

		this.observe(
			this.#workspaceContext.unique,
			(unique) => {
				this._elementUnique = unique!;
			},
			'_elementUnique',
		);
	}

	#renderStateTag() {
		switch (this._variant?.state) {
			case UmbElementVariantState.DRAFT:
				return html`
					<uui-tag look="secondary" label=${this.localize.term('content_unpublished')}>
						${this.localize.term('content_unpublished')}
					</uui-tag>
				`;
			case UmbElementVariantState.PUBLISHED:
			case UmbElementVariantState.PUBLISHED_PENDING_CHANGES:
				return html`
					<uui-tag color="positive" look="primary" label=${this.localize.term('content_published')}>
						${this.localize.term('content_published')}
					</uui-tag>
				`;
			case UmbElementVariantState.TRASHED:
				return html`
					<uui-tag color="danger" look="primary" label=${this.localize.term('content_trashed')}>
						${this.localize.term('content_trashed')}
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

		return html`
			<div class="general-item"><span>${this.#renderStateTag()}</span></div>
			${this.#renderCreateDate()} ${this.#renderUpdateDate()} ${this.#renderPublishDate()}

			<div class="general-item">
				<strong><umb-localize key="content_documentType">Document Type</umb-localize></strong>
				<uui-ref-node-document-type
					standalone
					href=${ifDefined(
						this._hasSettingsAccess ? editDocumentTypePath + 'edit/' + this._elementTypeUnique : undefined,
					)}
					?readonly=${!this._hasSettingsAccess}
					name=${ifDefined(this.localize.string(this._elementTypeName ?? ''))}>
					<umb-icon slot="icon" name=${ifDefined(this._elementTypeIcon)}></umb-icon>
				</uui-ref-node-document-type>
			</div>
			<div class="general-item">
				<strong><umb-localize key="template_id">Id</umb-localize></strong>
				<span>${this._elementUnique}</span>
			</div>
		`;
	}

	#renderCreateDate() {
		if (!this._variant?.createDate) return nothing;
		return this.#renderDate(this._variant.createDate, 'content_createDate', 'Created');
	}

	#renderUpdateDate() {
		if (!this._variant?.updateDate) return nothing;
		return this.#renderDate(this._variant.updateDate, 'content_updateDate', 'Last edited');
	}

	#renderPublishDate() {
		if (!this._variant?.publishDate) return nothing;
		return this.#renderDate(this._variant.publishDate, 'content_lastPublished', 'Last published');
	}

	#renderDate(date: string, labelKey: string, labelText: string) {
		return html`
			<div class="general-item">
				<strong><umb-localize .key=${labelKey}>${labelText}</umb-localize></strong>
				<span>
					<umb-localize-date .date=${date} .options=${TimeOptions}></umb-localize-date>
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

export default UmbElementWorkspaceViewInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-workspace-view-info': UmbElementWorkspaceViewInfoElement;
	}
}
