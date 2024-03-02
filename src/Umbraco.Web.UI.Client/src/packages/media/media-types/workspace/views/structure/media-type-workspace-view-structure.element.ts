import type { UmbMediaTypeWorkspaceContext } from '../../media-type-workspace.context.js';
import type { UmbInputMediaTypeElement } from '../../../components/input-media-type/input-media-type.element.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbContentTypeSortModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbInputCollectionConfigurationElement } from '@umbraco-cms/backoffice/components';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIToggleElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-media-type-workspace-view-structure')
export class UmbMediaTypeWorkspaceViewStructureElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: UmbMediaTypeWorkspaceContext;

	@state()
	private _allowedAtRoot?: boolean;

	@state()
	private _allowedContentTypeIDs?: Array<string>;

	@state()
	private _collection?: string | null;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (mediaTypeContext) => {
			this.#workspaceContext = mediaTypeContext as UmbMediaTypeWorkspaceContext;
			this._observeMediaType();
		});
	}

	private _observeMediaType() {
		if (!this.#workspaceContext) return;

		this.observe(
			this.#workspaceContext.allowedAtRoot,
			(allowedAtRoot) => (this._allowedAtRoot = allowedAtRoot),
			'_allowedAtRootObserver',
		);

		this.observe(
			this.#workspaceContext.allowedContentTypes,
			(allowedContentTypes) => {
				const oldValue = this._allowedContentTypeIDs;
				this._allowedContentTypeIDs = allowedContentTypes
					?.map((x) => x.contentType.unique)
					.filter((x) => x !== undefined) as Array<string>;
				this.requestUpdate('_allowedContentTypeIDs', oldValue);
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

	render() {
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
						<umb-input-media-type
							.selectedIds=${this._allowedContentTypeIDs ?? []}
							@change="${(e: CustomEvent) => {
								const sortedContentTypesList: Array<UmbContentTypeSortModel> = (
									e.target as UmbInputMediaTypeElement
								).selectedIds.map((id, index) => ({
									contentType: { unique: id },
									sortOrder: index,
								}));
								this.#workspaceContext?.setAllowedContentTypes(sortedContentTypesList);
							}}">
						</umb-input-media-type>
					</div>
				</umb-property-layout>
			</uui-box>
			<uui-box headline=${this.localize.term('contentTypeEditor_presentation')}>
				<umb-property-layout alias="collection" label="${this.localize.term('contentTypeEditor_collections')}">
					<div slot="description">${this.localize.term('contentTypeEditor_collectionsDescription')}</div>
					<div slot="editor">
						<umb-input-collection-configuration
							default-value="3a0156c4-3b8c-4803-bdc1-6871faa83fff"
							.value=${this._collection ?? ''}
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

	static styles = [
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

export default UmbMediaTypeWorkspaceViewStructureElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-view-structure': UmbMediaTypeWorkspaceViewStructureElement;
	}
}
