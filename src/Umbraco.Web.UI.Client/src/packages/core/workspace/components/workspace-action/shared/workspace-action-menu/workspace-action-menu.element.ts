import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestTypes, ManifestWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	#actionsInitializer?: UmbExtensionsElementInitializer<ManifestTypes, 'workspaceActionMenuItem'>;
	#workspaceActionAlias: Array<string> = [];

	@property({ type: Array })
	set workspaceActionAlias(alias: Array<string>) {
		// If there is an existing initializer, we need to dispose it.
		this.#actionsInitializer?.destroy();

		this.#workspaceActionAlias = alias;

		this.#actionsInitializer = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			'workspaceActionMenuItem', // TODO: Stop using string for 'workspaceActionMenuItem', we need to start using Const.
			(action) => action.meta.workspaceActionAliases.some((alias) => this.#workspaceActionAlias.includes(alias)),
			(ctrls) => {
				this._actions = ctrls;
			},
			'extensionsInitializer',
			'uui-menu-item',
		);
	}
	get workspaceActionAlias() {
		return this.#workspaceActionAlias;
	}

	@state()
	private _actions: Array<UmbExtensionElementInitializer<ManifestWorkspaceActionMenuItem, never>> = [];

	render() {
		return this._actions.length > 0
			? html`
					<uui-button
						id="popover-trigger"
						popovertarget="workspace-action-popover"
						look="secondary"
						label="Expand"
						compact>
						<uui-symbol-expand id="expand-symbol"></uui-symbol-expand>
					</uui-button>
					<uui-popover-container id="workspace-action-popover" placement="bottom-start">
						<umb-popover-layout>
							<div id="dropdown">${repeat(this._actions, (action) => action.component)}</div>
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
