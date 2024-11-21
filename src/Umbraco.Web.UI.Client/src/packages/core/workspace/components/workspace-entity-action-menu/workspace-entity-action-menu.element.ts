import { UMB_ENTITY_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing, query } from '@umbraco-cms/backoffice/external/lit';
import type { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
@customElement('umb-workspace-entity-action-menu')
export class UmbWorkspaceEntityActionMenuElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _unique?: UmbEntityUnique;

	@state()
	private _entityType?: string;

	@state()
	private _popoverOpen = false;

	@query('#workspace-entity-action-menu-popover')
	private _popover?: UUIPopoverContainerElement;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this.observe(this._workspaceContext.unique, (unique) => {
				this._unique = unique;
				// TODO: the context does not have an observable for the entity type, so we need to use the
				// getEntityType method until we can add an observable for it.
				this._entityType = this._workspaceContext?.getEntityType();
			});
		});
	}

	#onActionExecuted(event: UmbActionExecutedEvent) {
		event.stopPropagation();

		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popover?.hidePopover();
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		return this._unique !== undefined && this._entityType
			? html`
					<uui-button
						id="action-button"
						popovertarget="workspace-entity-action-menu-popover"
						label=${this.localize.term('general_actions')}>
						${this.localize.term('general_actions')}
						<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
					</uui-button>
					<uui-popover-container
						id="workspace-entity-action-menu-popover"
						placement="bottom-end"
						@toggle=${this.#onPopoverToggle}>
						<umb-popover-layout>
							<uui-scroll-container>
								<umb-entity-action-list
									@action-executed=${this.#onActionExecuted}
									.entityType=${this._entityType}
									.unique=${this._unique}>
								</umb-entity-action-list>
							</uui-scroll-container>
						</umb-popover-layout>
					</uui-popover-container>
				`
			: nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				height: 100%;
			}

			:host > uui-button {
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
