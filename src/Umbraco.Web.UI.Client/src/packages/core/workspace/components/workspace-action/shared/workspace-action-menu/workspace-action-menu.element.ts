import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestTypes, ManifestWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_WORKSPACE_CONTEXT.TYPE;
	#actionsInitializer?: UmbExtensionsElementInitializer<ManifestTypes, 'workspaceActionMenuItem'>;

	@property({ type: Array })
	workspaceActionAlias: Array<string> = [];

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

		const unique = this.#workspaceContext.getUnique();
		const entityType = this.#workspaceContext.getEntityType();

		this.#actionsInitializer = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			'workspaceActionMenuItem', // TODO: Stop using string for 'workspaceActionMenuItem', we need to start using Const.
			(action) =>
				action.meta.workspaceActionAliases.some((alias) => this.workspaceActionAlias.includes(alias)) &&
				action.meta.entityTypes.includes(entityType),
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
						look="secondary"
						label="Expand"
						compact>
						<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
					</uui-button>
					<uui-popover-container id="workspace-action-popover" placement="bottom-end" @toggle=${this.#onPopoverToggle}>
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
			#more-symbol {
				font-size: 0.6em;
			}

			#popover-trigger {
				--uui-button-padding-top-factor: 0.5;
				--uui-button-padding-bottom-factor: 0.1;
				--uui-button-height: 18px;
				--uui-button-border-radius: 6px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
