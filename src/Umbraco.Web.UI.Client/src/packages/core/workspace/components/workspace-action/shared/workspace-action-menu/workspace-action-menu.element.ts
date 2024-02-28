import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestTypes, ManifestWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_WORKSPACE_CONTEXT.TYPE;
	#actionsInitializer?: UmbExtensionsElementInitializer<ManifestTypes, 'workspaceActionMenuItem'>;

	/**
	 * The workspace actions to filter the available actions by.
	 * @example ['Umb.WorkspaceAction.Document.Save', 'Umb.WorkspaceAction.Document.SaveAndPublishNew']
	 */
	@property({ type: Array })
	workspaceActions: Array<string> = [];

	@property()
	look: UUIInterfaceLook = 'secondary';

	@property()
	color: UUIInterfaceColor = 'default';

	@state()
	private _actions: Array<UmbExtensionElementInitializer<ManifestWorkspaceActionMenuItem, never>> = [];

	@state()
	_popoverOpen = false;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#initialise();
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#actionsInitializer?.destroy();
	}

	#initialise() {
		if (!this.#workspaceContext) throw new Error('No workspace context');

		// If there are no workspace action aliases, then there is no need to initialize the actions.
		if (!this.workspaceActions.length) return;

		const unique = this.#workspaceContext.getUnique();
		const entityType = this.#workspaceContext.getEntityType();

		this.#actionsInitializer = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			'workspaceActionMenuItem', // TODO: Stop using string for 'workspaceActionMenuItem', we need to start using Const.
			(action) => {
				const containsAlias = action.meta.workspaceActions.some((alias) => this.workspaceActions.includes(alias));
				const isValidEntityType = !action.meta.entityTypes.length || action.meta.entityTypes.includes(entityType);
				return containsAlias && isValidEntityType;
			},
			(ctrls) => {
				ctrls.forEach((ctrl) => {
					ctrl.properties = { unique, entityType };
				});
				this._actions = ctrls;
			},
			undefined,
			'umb-entity-action',
		);
	}

	// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#onPopoverToggle(event: ToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	render() {
		return this._actions.length > 0
			? html`
					<uui-button
						id="popover-trigger"
						popovertarget="workspace-action-popover"
						look="${this.look}"
						color="${this.color}"
						label="Expand"
						compact>
						<uui-symbol-expand id="expand-symbol" .open=${this._popoverOpen}></uui-symbol-expand>
					</uui-button>
					<uui-popover-container
						id="workspace-action-popover"
						margin="5"
						placement="top-end"
						@toggle=${this.#onPopoverToggle}>
						<umb-popover-layout>
							<uui-scroll-container>
								${repeat(
									this._actions,
									(action) => action.alias,
									(action) => action.component,
								)}
							</uui-scroll-container>
						</umb-popover-layout>
					</uui-popover-container>
			  `
			: nothing;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			#expand-symbol {
				transform: rotate(-90deg);
			}

			#expand-symbol[open] {
				transform: rotate(0deg);
			}

			#workspace-action-popover {
				min-width: 200px;
			}

			#popover-trigger {
				--uui-button-padding-top-factor: 0.5;
				--uui-button-padding-bottom-factor: 0.1;
				--uui-button-border-radius: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
