import type { UmbWorkspaceAction } from '../workspace-action.interface.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	ManifestWorkspaceAction,
	MetaWorkspaceActionDefaultKind,
} from '@umbraco-cms/backoffice/extension-registry';

import '../../workspace-action-menu/index.js';

@customElement('umb-workspace-action')
export class UmbWorkspaceActionElement<
	MetaType extends MetaWorkspaceActionDefaultKind = MetaWorkspaceActionDefaultKind,
	ApiType extends UmbWorkspaceAction<MetaType> = UmbWorkspaceAction<MetaType>,
> extends UmbLitElement {
	#manifest?: ManifestWorkspaceAction<MetaType>;
	#api?: ApiType;

	@state()
	private _buttonState?: UUIButtonState;

	@state()
	private _aliases: Array<string> = [];

	@state()
	_href?: string;

	@state()
	_isDisabled = false;

	@property({ type: Object, attribute: false })
	public set manifest(value: ManifestWorkspaceAction<MetaType> | undefined) {
		if (!value) return;
		const oldValue = this.#manifest;
		this.#manifest = value;
		if (oldValue !== this.#manifest) {
			this.#createAliases();
			this.requestUpdate('manifest', oldValue);
		}
	}
	public get manifest() {
		return this.#manifest;
	}

	@property({ attribute: false })
	public set api(api: ApiType | undefined) {
		this.#api = api;

		// TODO: Fix so when we use a HREF it does not refresh the page?
		this.#api?.getHref?.().then((href) => {
			this._href = href;
			// TODO: Do we need to update the component here? [NL]
		});

		this.#observeIsDisabled();
	}
	public get api(): ApiType | undefined {
		return this.#api;
	}

	/**
	 * Create a list of original and overwritten aliases of workspace actions for the action.
	 */
	async #createAliases() {
		if (!this.#manifest) return;
		const aliases = new Set<string>();
		if (this.#manifest) {
			aliases.add(this.#manifest.alias);

			// TODO: This works on one level for now, which will be enough for the current use case. However, you can overwrite the overwrites, so we need to make this recursive. Perhaps we could move this to the extensions initializer.
			// Add overwrites so that we can show any previously registered actions on the original workspace action
			if (this.#manifest.overwrites) {
				for (const alias of this.#manifest.overwrites) {
					aliases.add(alias);
				}
			}
		}
		this._aliases = Array.from(aliases);
	}

	private async _onClick(event: MouseEvent) {
		if (this._href) {
			event.stopPropagation();
		}

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

	#observeIsDisabled() {
		this.observe(
			this.#api?.isDisabled,
			(isDisabled) => {
				this._isDisabled = isDisabled || false;
			},
			'isDisabledObserver',
		);
	}

	override render() {
		return html`
			<uui-button-group>
				<uui-button
					id="action-button"
					.href=${this._href}
					@click=${this._onClick}
					look=${this.#manifest?.meta.look || 'default'}
					color=${this.#manifest?.meta.color || 'default'}
					label=${ifDefined(
						this.#manifest?.meta.label ? this.localize.string(this.#manifest.meta.label) : this.#manifest?.name,
					)}
					.disabled=${this._isDisabled}
					.state=${this._buttonState}></uui-button>
				<umb-workspace-action-menu
					.forWorkspaceActions=${this._aliases}
					color="${this.#manifest?.meta.color || 'default'}"
					look="${this.#manifest?.meta.look || 'default'}"></umb-workspace-action-menu>
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
