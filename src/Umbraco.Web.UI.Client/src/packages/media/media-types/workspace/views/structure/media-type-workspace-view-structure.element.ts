import type { UmbInputMediaTypeElement } from '../../../components/input-media-type/input-media-type.element.js';
import { UMB_MEDIA_TYPE_WORKSPACE_CONTEXT } from '../../media-type-workspace.context-token.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../../entity.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '@umbraco-cms/backoffice/schema-lockdown';
import type {
	UmbContentTypeSortModel,
	UmbInputContentTypeCollectionConfigurationElement,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UUIToggleElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-media-type-workspace-view-structure')
export class UmbMediaTypeWorkspaceViewStructureElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: typeof UMB_MEDIA_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _allowedAtRoot?: boolean;

	@state()
	private _allowedContentTypeIDs?: Array<string>;

	@state()
	private _collection?: string;

	// Restricted until the schema lockdown matrix confirms the operation is allowed (safe default).
	@state()
	private _isRestricted = true;

	constructor() {
		super();
		// When the schema is locked down the view is read-only. Making the whole view inert disables every
		// input/toggle/button at once without wiring each one, and the dimmed styling signals it is read-only.
		this.consumeContext(UMB_SCHEMA_LOCKDOWN_CONTEXT, (context) => {
			this.observe(
				context?.state,
				() => {
					this._isRestricted = context?.isAllowed(UMB_MEDIA_TYPE_ENTITY_TYPE, 'update') !== true;
					this.inert = this._isRestricted;
				},
				'_observeSchemaLockdown',
			);
		});

		this.consumeContext(UMB_MEDIA_TYPE_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
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
					.filter((x) => x !== undefined);
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

	#renderSchemaLockdownNotice() {
		if (!this._isRestricted) return nothing;
		return html`
			<uui-box id="schema-lockdown-notice">
				<div class="notice">
					<umb-icon name="icon-info"></umb-icon>
					<div>
						<strong><umb-localize key="schemaLockdown_headline">Schema Locked</umb-localize></strong>
						<p><umb-localize key="schemaLockdown_notice"></umb-localize></p>
					</div>
				</div>
			</uui-box>
		`;
	}

	override render() {
		return html`
			${this.#renderSchemaLockdownNotice()}
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
							.selection=${this._allowedContentTypeIDs ?? []}
							@change="${(e: CustomEvent & { target: UmbInputMediaTypeElement }) => {
								const sortedContentTypesList: Array<UmbContentTypeSortModel> = e.target.selection.map((id, index) => ({
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
						<umb-input-content-type-collection-configuration
							default-value="3a0156c4-3b8c-4803-bdc1-6871faa83fff"
							.value=${this._collection}
							@change=${(e: CustomEvent) => {
								const unique = (e.target as UmbInputContentTypeCollectionConfigurationElement).value as string;
								this.#workspaceContext?.setCollection({ unique });
							}}>
						</umb-input-content-type-collection-configuration>
					</div>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host([inert]) > :not(#schema-lockdown-notice) {
				opacity: 0.6;
			}
			#schema-lockdown-notice {
				--uui-box-default-padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border-left: 4px solid var(--uui-color-warning-standalone, #f0ac00);
				margin-bottom: var(--uui-size-layout-1);
			}
			#schema-lockdown-notice .notice {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: flex-start;
			}
			#schema-lockdown-notice umb-icon {
				flex: 0 0 auto;
				font-size: var(--uui-size-6);
				margin-top: 2px;
				color: var(--uui-color-warning-standalone, #f0ac00);
			}
			#schema-lockdown-notice p {
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
		`,
	];
}

export default UmbMediaTypeWorkspaceViewStructureElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-view-structure': UmbMediaTypeWorkspaceViewStructureElement;
	}
}
