import { html, customElement, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
@customElement('umb-workspace-entity-action-menu')
export class UmbWorkspaceEntityActionMenuElement extends UmbLitElement {
	@state()
	private _unique?: UmbEntityUnique;

	@state()
	private _entityType?: string;

	@state()
	private _hasActions = false;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.entityType,
				(entityType) => {
					this._entityType = entityType;
					this.#observeEntityActions(entityType);
				},
				'_observeEntityType',
			);
			this.observe(context.unique, (unique) => (this._unique = unique), '_observeUnique');
		});
	}

	#observeEntityActions(entityType: string | undefined) {
		if (!entityType) {
			this.removeUmbControllerByAlias('umbEntityActionsObserver');
			this._hasActions = false;
			return;
		}

		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'entityAction',
			(ext) => ext.forEntityTypes.includes(entityType),
			(actions) => (this._hasActions = actions.length > 0),
			'umbEntityActionsObserver',
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
