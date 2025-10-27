import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import { css, customElement, html, ifDefined, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestWorkspaceView, UmbActiveVariant } from '@umbraco-cms/backoffice/workspace';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';

import './document-workspace-split-view-variant-selector.element.js';
import { UMB_ROUTE_CONTEXT } from '@umbraco-cms/backoffice/router';

@customElement('umb-document-workspace-split-view')
export class UmbDocumentWorkspaceSplitViewElement extends UmbLitElement {
	// TODO: Refactor: use the split view context token:
	private _workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _variants?: Array<UmbActiveVariant>;

	@state()
	private _icon?: string;

	@state()
	private _overrides?: Array<UmbDeepPartialObject<ManifestWorkspaceView>>;

	@state()
	private _loading = true;

	constructor() {
		super();

		// TODO: Refactor: use a split view workspace context token: [NL]
		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this.#observeActiveVariantInfo();
			this.#observeIcon();
			this.#observeLoading();
			this.#observeCollectionOverrides();
		});

		// TODO: Make sure this works when opening a collection when document is already open.
		// get current get variables from url, and check if openCollection is set:
		const urlSearchParams = new URLSearchParams(window.location.search);
		const openCollection = urlSearchParams.has('openCollection');
		if (openCollection) {
			this.getContext(UMB_ROUTE_CONTEXT).then((routeContext) => {
				if (routeContext) {
					window.history.replaceState({}, '', routeContext.getActivePath() + '/view/collection');
				}
			});
		}
	}

	#observeActiveVariantInfo() {
		this.observe(
			this._workspaceContext?.splitView.activeVariantsInfo,
			(variants) => {
				this._variants = variants;
			},
			'_observeActiveVariantsInfo',
		);
	}

	#observeIcon() {
		this.observe(
			this._workspaceContext?.contentTypeIcon,
			(icon) => {
				this._icon = icon ?? undefined;
			},
			'observeIcon',
		);
	}

	#observeLoading() {
		this.observe(
			this._workspaceContext?.loading.isOn,
			(loading) => {
				this._loading = loading ?? false;
			},
			'observeIcon',
		);
	}

	#observeCollectionOverrides() {
		this.observe(
			this._workspaceContext?.collection.manifestOverrides,
			(overrides) => {
				this._overrides = overrides ? [overrides] : undefined;
			},
			'observeCollectionOverrides',
		);
	}

	override render() {
		return this._variants
			? html`<div id="splitViews">
						${repeat(
							this._variants,
							(view) =>
								view.index + '_' + (view.culture ?? '') + '_' + (view.segment ?? '') + '_' + this._variants!.length,
							(view) => html`
								<umb-workspace-split-view
									.loading=${this._loading}
									.displayNavigation=${view.index === this._variants!.length - 1}
									.overrides=${this._overrides}
									.splitViewIndex=${view.index}>
									<umb-icon slot="icon" name=${ifDefined(this._icon)}></umb-icon>
									<umb-document-workspace-split-view-variant-selector
										slot="variant-selector"></umb-document-workspace-split-view-variant-selector>
								</umb-workspace-split-view>
							`,
						)}
					</div>

					<umb-workspace-footer alias="Umb.Workspace.Document"></umb-workspace-footer>`
			: nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				width: 100%;
				height: 100%;

				display: flex;
				flex: 1;
				flex-direction: column;
			}

			#splitViews {
				display: flex;
				width: 100%;
				height: calc(100% - var(--umb-footer-layout-height));
			}
		`,
	];
}

export default UmbDocumentWorkspaceSplitViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-split-view': UmbDocumentWorkspaceSplitViewElement;
	}
}
