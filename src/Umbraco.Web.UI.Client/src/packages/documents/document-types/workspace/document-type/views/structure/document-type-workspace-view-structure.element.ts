import type { UmbInputDocumentTypeElement } from '../../../../components/input-document-type/input-document-type.element.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from '../../document-type-workspace.context-token.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type {
	UmbContentTypeSortModel,
	UmbInputContentTypeCollectionConfigurationElement,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UUIToggleElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-type-workspace-view-structure')
export class UmbDocumentTypeWorkspaceViewStructureElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: typeof UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _allowedAtRoot?: boolean;

	@state()
	private _allowedContentTypeUniques?: Array<string>;

	@state()
	private _collection?: string | null;

	@state()
	private _isElement?: boolean;

	// Restricted until the server confirms it is not in production runtime mode (safe default).
	@state()
	private _isRestricted = true;

	constructor() {
		super();
		// In production runtime mode the schema is read-only. Making the whole view inert disables every
		// input/toggle/button at once without wiring each one, and the dimmed styling signals it is read-only.
		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.observe(
				context?.isProductionMode,
				(isProductionMode) => {
					this._isRestricted = isProductionMode !== false;
					this.inert = this._isRestricted;
				},
				'_observeProductionMode',
			);
		});

		this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this.#workspaceContext = documentTypeContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.allowedAtRoot,
			(allowedAtRoot) => (this._allowedAtRoot = allowedAtRoot),
			'_allowedAtRootObserver',
		);

		this.observe(this.#workspaceContext.isElement, (isElement) => (this._isElement = isElement), '_isElementObserver');

		this.observe(
			this.#workspaceContext.allowedContentTypes,
			(allowedContentTypes) => {
				const oldValue = this._allowedContentTypeUniques;
				this._allowedContentTypeUniques = allowedContentTypes
					?.map((x) => x.contentType.unique)
					.filter((x) => x !== undefined) as Array<string>;
				this.requestUpdate('_allowedContentTypeUniques', oldValue);
			},
			'_allowedContentTypesObserver',
		);

		this.observe(
			this.#workspaceContext.collection,
			(collection) => {
				this._collection = collection?.unique;
			},
			'_collectionObserver',
		);
	}

	#renderProductionModeNotice() {
		if (!this._isRestricted) return nothing;
		return html`
			<uui-box id="production-mode-notice">
				<div class="notice">
					<umb-icon name="icon-info"></umb-icon>
					<div>
						<strong><umb-localize key="general_productionMode">Production Mode</umb-localize></strong>
						<p><umb-localize key="general_runtimeModeProductionSchema"></umb-localize></p>
					</div>
				</div>
			</uui-box>
		`;
	}

	override render() {
		return html`
			${this.#renderProductionModeNotice()}
			<uui-box headline=${this.localize.term('contentTypeEditor_structure')}>
				<umb-property-layout alias="Root" label=${this.localize.term('contentTypeEditor_allowAtRootHeading')}>
					<div slot="description">${this.localize.term('contentTypeEditor_allowAtRootDescription')}</div>
					<div slot="editor">
						<uui-toggle
							label=${this.localize.term('contentTypeEditor_allowAtRootHeading')}
							?checked=${this._allowedAtRoot}
							?disabled=${this._isElement}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setAllowedAtRoot((e.target as UUIToggleElement).checked);
							}}></uui-toggle>
					</div>
				</umb-property-layout>
				<umb-property-layout alias="ChildNodeType" label=${this.localize.term('contentTypeEditor_childNodesHeading')}>
					<div slot="description">${this.localize.term('contentTypeEditor_childNodesDescription')}</div>
					<div slot="editor">
						${this._isElement
							? html`
									<div class="not-applicable-message">
										<umb-localize key="contentTypeEditor_elementDoesNotSupport">
											This is not applicable for an Element type.
										</umb-localize>
									</div>
								`
							: html`
									<!-- TODO: maybe we want to somehow display the hierarchy, but not necessary in the same way as old backoffice? -->
									<umb-input-document-type
										.documentTypesOnly=${true}
										.selection=${this._allowedContentTypeUniques ?? []}
										@change="${(e: CustomEvent & { target: UmbInputDocumentTypeElement }) => {
											const sortedContentTypesList: Array<UmbContentTypeSortModel> = e.target.selection.map(
												(id, index) => ({
													contentType: { unique: id },
													sortOrder: index,
												}),
											);
											this.#workspaceContext?.setAllowedContentTypes(sortedContentTypesList);
										}}">
									</umb-input-document-type>
								`}
					</div>
				</umb-property-layout>
			</uui-box>
			<uui-box headline=${this.localize.term('contentTypeEditor_presentation')}>
				<umb-property-layout alias="collection" label="${this.localize.term('contentTypeEditor_collection')}">
					<div slot="description">${this.localize.term('contentTypeEditor_collectionDescription')}</div>
					<div slot="editor">
						${this._isElement
							? html`
									<div class="not-applicable-message">
										<umb-localize key="contentTypeEditor_elementDoesNotSupport">
											This is not applicable for an Element type.
										</umb-localize>
									</div>
								`
							: html`
									<umb-input-content-type-collection-configuration
										default-value="c0808dd3-8133-4e4b-8ce8-e2bea84a96a4"
										.value=${this._collection ?? undefined}
										@change=${(e: CustomEvent) => {
											const unique = (e.target as UmbInputContentTypeCollectionConfigurationElement).value as string;
											this.#workspaceContext?.setCollection({ unique });
										}}>
									</umb-input-content-type-collection-configuration>
								`}
					</div>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host([inert]) > :not(#production-mode-notice) {
				opacity: 0.6;
			}
			#production-mode-notice {
				--uui-box-default-padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border-left: 4px solid var(--uui-color-warning-standalone, #f0ac00);
				margin-bottom: var(--uui-size-layout-1);
			}
			#production-mode-notice .notice {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: flex-start;
			}
			#production-mode-notice umb-icon {
				flex: 0 0 auto;
				font-size: var(--uui-size-6);
				margin-top: 2px;
				color: var(--uui-color-warning-standalone, #f0ac00);
			}
			#production-mode-notice p {
				margin: var(--uui-size-space-2) 0 0;
			}
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}
			uui-box {
				margin-top: var(--uui-size-layout-1);
			}
			uui-label,
			umb-property-editor-ui-number {
				display: block;
			}

			// TODO: is this necessary?
			uui-toggle {
				display: flex;
			}

			.not-applicable-message {
				color: var(--uui-color-text-alt);
				font-style: italic;
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewStructureElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-structure': UmbDocumentTypeWorkspaceViewStructureElement;
	}
}
