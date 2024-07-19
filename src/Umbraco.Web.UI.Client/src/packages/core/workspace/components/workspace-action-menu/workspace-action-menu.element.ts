import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	umbExtensionsRegistry,
	type ManifestWorkspaceActionMenuItem,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import {
	type UmbExtensionElementAndApiInitializer,
	UmbExtensionsElementAndApiInitializer,
} from '@umbraco-cms/backoffice/extension-api';

function ExtensionApiArgsMethod(manifest: ManifestWorkspaceActionMenuItem) {
	return [{ meta: manifest.meta }];
}
@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementAndApiInitializer<
		ManifestWorkspaceActionMenuItem,
		'workspaceActionMenuItem',
		ManifestWorkspaceActionMenuItem
	>;

	/**
	 * The workspace actions to filter the available actions by.
	 * @example ['Umb.WorkspaceAction.Document.Save', 'Umb.WorkspaceAction.Document.SaveAndPublishNew']
	 */
	@property({ attribute: false })
	public set forWorkspaceActions(value: Array<string>) {
		if (value === this._forWorkspaceActions) return;
		this._forWorkspaceActions = value;
		this._filter = (action) => {
			return Array.isArray(action.forWorkspaceActions)
				? action.forWorkspaceActions.some((alias) => this.forWorkspaceActions.includes(alias))
				: this.forWorkspaceActions.includes(action.forWorkspaceActions);
		};
		this.#observeExtensions();
	}
	public get forWorkspaceActions(): Array<string> {
		return this._forWorkspaceActions;
	}
	private _forWorkspaceActions: Array<string> = [];

	@state()
	_filter?: (action: ManifestWorkspaceActionMenuItem) => boolean;

	@property()
	look: UUIInterfaceLook = 'secondary';

	@property()
	color: UUIInterfaceColor = 'default';

	@state()
	_items: Array<UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>> = [];

	@state()
	_popoverOpen = false;

	#observeExtensions(): void {
		this.#extensionsController?.destroy();
		if (this._filter) {
			this.#extensionsController = new UmbExtensionsElementAndApiInitializer<
				ManifestWorkspaceActionMenuItem,
				'workspaceActionMenuItem',
				ManifestWorkspaceActionMenuItem
			>(
				this,
				umbExtensionsRegistry,
				'workspaceActionMenuItem',
				ExtensionApiArgsMethod,
				this._filter,
				(extensionControllers) => {
					this._items = extensionControllers;
				},
				undefined, // We can leave the alias to undefined, as we destroy this our selfs.
			);
			//this.#extensionsController.elementProperties = this.#elProps;
		}
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		return this._items && this._items.length > 0
			? html`
					<uui-button
						id="popover-trigger"
						popovertarget="workspace-action-popover"
						look="${this.look}"
						color="${this.color}"
						label=${this.localize.term('visuallyHiddenTexts_tabExpand')}
						compact>
						<uui-symbol-expand id="expand-symbol" .open=${this._popoverOpen}></uui-symbol-expand>
					</uui-button>
					<uui-popover-container
						id="workspace-action-popover"
						margin="6"
						placement="top-end"
						@toggle=${this.#onPopoverToggle}>
						<umb-popover-layout>
							<uui-scroll-container>
								${this._items.length > 0
									? repeat(
											this._items,
											(ext) => ext.alias,
											(ext) => ext.component,
										)
									: ''}
							</uui-scroll-container>
						</umb-popover-layout>
					</uui-popover-container>
				`
			: nothing;
	}

	static override styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			#expand-symbol {
				/* TODO: remove this hack and use a proper UUI symbol for this */
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
