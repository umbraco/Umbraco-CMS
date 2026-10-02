import type { ManifestWorkspaceInfoAppEntityReferencesKind } from './types.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

import '../../global-components/entity-reference-list.element.js';

@customElement('umb-entity-references-workspace-info-app')
export class UmbEntityReferencesWorkspaceInfoAppElement extends UmbLitElement {
	@property({ type: Object })
	private _manifest?: ManifestWorkspaceInfoAppEntityReferencesKind | undefined;
	public get manifest(): ManifestWorkspaceInfoAppEntityReferencesKind | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestWorkspaceInfoAppEntityReferencesKind | undefined) {
		this._manifest = value;
	}

	@state()
	private _unique?: UmbEntityUnique;

	#workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeUnique();
		});
	}

	#observeUnique() {
		this.observe(
			this.#workspaceContext?.unique,
			(unique) => {
				this._unique = unique ?? undefined;
			},
			'umbEntityReferencesUniqueObserver',
		);
	}

	override render() {
		return html`
			<umb-workspace-info-app-layout headline="#references_labelUsedByItems">
				<div id="content">
					<umb-entity-reference-list
						.unique=${this._unique ?? undefined}
						.referenceRepositoryAlias=${this._manifest?.meta.referenceRepositoryAlias}
						source="referencedBy">
					</umb-entity-reference-list>
				</div>
			</umb-workspace-info-app-layout>
		`;
	}

	static override styles = [
		css`
			:host {
				display: contents;
			}

			#content {
				display: block;
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbEntityReferencesWorkspaceInfoAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-references-workspace-info-app': UmbEntityReferencesWorkspaceInfoAppElement;
	}
}
