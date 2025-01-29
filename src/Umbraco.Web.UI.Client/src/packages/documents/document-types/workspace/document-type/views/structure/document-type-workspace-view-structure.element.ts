import type { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import type { UmbInputDocumentTypeElement } from '../../../../components/input-document-type/input-document-type.element.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from '../../document-type-workspace.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbContentTypeSortModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbInputCollectionConfigurationElement } from '@umbraco-cms/backoffice/components';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UUIToggleElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-type-workspace-view-structure')
export class UmbDocumentTypeWorkspaceViewStructureElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: UmbDocumentTypeWorkspaceContext;

	@state()
	private _allowedAtRoot?: boolean;

	@state()
	private _allowedContentTypeUniques?: Array<string>;

	@state()
	private _collection?: string | null;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this.#workspaceContext = documentTypeContext as UmbDocumentTypeWorkspaceContext;
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

	override render() {
		return html`
			<uui-box headline=${this.localize.term('contentTypeEditor_structure')}>
				<umb-property-layout alias="Root" label=${this.localize.term('contentTypeEditor_allowAtRootHeading')}>
					<div slot="description">${this.localize.term('contentTypeEditor_allowAtRootDescription')}</div>
					<div slot="editor">
						<uui-toggle
							label=${this.localize.term('contentTypeEditor_allowAtRootHeading')}
							?checked=${this._allowedAtRoot}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setAllowedAtRoot((e.target as UUIToggleElement).checked);
							}}></uui-toggle>
					</div>
				</umb-property-layout>
				<umb-property-layout alias="ChildNodeType" label=${this.localize.term('contentTypeEditor_childNodesHeading')}>
					<div slot="description">${this.localize.term('contentTypeEditor_childNodesDescription')}</div>
					<div slot="editor">
						<!-- TODO: maybe we want to somehow display the hierarchy, but not necessary in the same way as old backoffice? -->
						<umb-input-document-type
							.documentTypesOnly=${true}
							.selection=${this._allowedContentTypeUniques ?? []}
							@change="${(e: CustomEvent & { target: UmbInputDocumentTypeElement }) => {
								const sortedContentTypesList: Array<UmbContentTypeSortModel> = e.target.selection.map((id, index) => ({
									contentType: { unique: id },
									sortOrder: index,
								}));
								this.#workspaceContext?.setAllowedContentTypes(sortedContentTypesList);
							}}">
						</umb-input-document-type>
					</div>
				</umb-property-layout>
			</uui-box>
			<uui-box headline=${this.localize.term('contentTypeEditor_presentation')}>
				<umb-property-layout alias="collection" label="${this.localize.term('contentTypeEditor_collection')}">
					<div slot="description">${this.localize.term('contentTypeEditor_collectionDescription')}</div>
					<div slot="editor">
						<umb-input-collection-configuration
							default-value="c0808dd3-8133-4e4b-8ce8-e2bea84a96a4"
							.value=${this._collection ?? undefined}
							@change=${(e: CustomEvent) => {
								const unique = (e.target as UmbInputCollectionConfigurationElement).value as string;
								this.#workspaceContext?.setCollection({ unique });
							}}>
						</umb-input-collection-configuration>
					</div>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
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
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewStructureElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-structure': UmbDocumentTypeWorkspaceViewStructureElement;
	}
}
