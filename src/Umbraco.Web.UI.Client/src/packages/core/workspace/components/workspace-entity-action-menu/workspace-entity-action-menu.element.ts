import { UMB_ENTITY_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { html, customElement, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
@customElement('umb-workspace-entity-action-menu')
export class UmbWorkspaceEntityActionMenuElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _unique?: UmbEntityUnique;

	@state()
	private _entityType?: string;

	@state()
	private _hasActions = false;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this.observe(this._workspaceContext?.unique, (unique) => {
				this._unique = unique;
				// TODO: the context does not have an observable for the entity type, so we need to use the
				// getEntityType method until we can add an observable for it.
				const entityType = this._workspaceContext?.getEntityType();
				if (entityType !== this._entityType) {
					this._entityType = entityType;
					this.#observeEntityActions();
				}
			});
		});
	}

	#observeEntityActions() {
		if (!this._entityType) return;
		const entityType = this._entityType;

		// Hide the menu when no entity actions are available for this entity type, so it doesn't open an empty dropdown.
		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'entityAction',
			(ext) => ext.forEntityTypes.includes(entityType),
			(actions) => {
				this._hasActions = actions.length > 0;
			},
			'umbWorkspaceEntityActionsObserver',
		);
	}

	override render() {
		if (!this._entityType) return nothing;
		if (this._unique === undefined) return nothing;
		if (!this._hasActions) return nothing;

		return html`<umb-entity-actions-dropdown
			data-mark="workspace:action-menu-button"
			label=${this.localize.term('general_actions')}>
			<uui-symbol-more slot="label"></uui-symbol-more>
		</umb-entity-actions-dropdown>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				height: 100%;
				margin-left: calc(var(--uui-size-layout-1) * -1);
			}

			umb-entity-actions-dropdown {
				height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-entity-action-menu': UmbWorkspaceEntityActionMenuElement;
	}
}
