import type { UmbWorkspaceAction } from '../../../../index.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

import '../workspace-action-menu/index.js';

@customElement('umb-workspace-action')
export class UmbWorkspaceActionElement extends UmbLitElement {
	#manifest?: ManifestWorkspaceAction;

	@state()
	private _buttonState?: UUIButtonState;

	@state()
	private _aliases: Array<string> = [];

	@property({ type: Object, attribute: false })
	public get manifest() {
		return this.#manifest;
	}
	public set manifest(value: ManifestWorkspaceAction | undefined) {
		if (!value) return;
		const oldValue = this.#manifest;
		this.#manifest = value;
		if (oldValue !== this.#manifest) {
			this.#createApi();
			this.#createAliases();
			this.requestUpdate('manifest', oldValue);
		}
	}

	async #createApi() {
		if (!this.manifest) return;
		this.#api = await createExtensionApi(this.manifest, [this]);
	}

	/**
	 * Create a list of original and overwritten aliases of workspace actions for the action.
	 */
	async #createAliases() {
		if (!this.manifest) return;
		const aliases = new Set<string>();
		if (this.manifest) {
			aliases.add(this.manifest.alias);

			// TODO: This works on one level for now, which will be enough for the current use case. However, you can overwrite the overwrites, so we need to make this recursive. Perhaps we could move this to the extensions initializer.
			// Add overwrites so that we can show any previously registered actions on the original workspace action
			if (this.manifest.overwrites) {
				for (const alias of this.manifest.overwrites) {
					aliases.add(alias);
				}
			}
		}
		this._aliases = Array.from(aliases);
	}

	#api?: UmbWorkspaceAction;

	private async _onClick() {
		this._buttonState = 'waiting';

		try {
			if (!this.#api) throw new Error('No api defined');
			await this.#api.execute();
			this._buttonState = 'success';
		} catch (error) {
			this._buttonState = 'failed';
		}

		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	render() {
		return html`
			<uui-button-group>
				<uui-button
					id="action-button"
					@click=${this._onClick}
					look=${this.manifest?.meta.look || 'default'}
					color=${this.manifest?.meta.color || 'default'}
					label=${this.manifest?.meta.label || ''}
					.state=${this._buttonState}></uui-button>
				<umb-workspace-action-menu
					.workspaceActions=${this._aliases}
					color="${this.manifest?.meta.color || 'default'}"
					look="${this.manifest?.meta.look || 'default'}"></umb-workspace-action-menu>
			</uui-button-group>
		`;
	}
}

export default UmbWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action': UmbWorkspaceActionElement;
	}
}
