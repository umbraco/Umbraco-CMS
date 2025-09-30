import { UMB_ENTITY_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { html, customElement, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
@customElement('umb-workspace-entity-action-menu')
export class UmbWorkspaceEntityActionMenuElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _unique?: UmbEntityUnique;

	@state()
	private _entityType?: string;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this.observe(this._workspaceContext?.unique, (unique) => {
				this._unique = unique;
				// TODO: the context does not have an observable for the entity type, so we need to use the
				// getEntityType method until we can add an observable for it.
				this._entityType = this._workspaceContext?.getEntityType();
			});
		});
	}

	override render() {
		if (!this._entityType) return nothing;
		if (this._unique === undefined) return nothing;

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
