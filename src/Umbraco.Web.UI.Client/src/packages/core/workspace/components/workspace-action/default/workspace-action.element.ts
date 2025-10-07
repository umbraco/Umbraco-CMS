import type {
	ManifestWorkspaceAction,
	ManifestWorkspaceActionMenuItem,
	MetaWorkspaceActionDefaultKind,
	UmbWorkspaceActionArgs,
	UmbWorkspaceActionDefaultKind,
} from '../../../types.js';
import { customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { stringOrStringArrayIntersects } from '@umbraco-cms/backoffice/utils';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbAction } from '@umbraco-cms/backoffice/action';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-workspace-action')
export class UmbWorkspaceActionElement<
	MetaType extends MetaWorkspaceActionDefaultKind = MetaWorkspaceActionDefaultKind,
	ApiType extends UmbWorkspaceActionDefaultKind<MetaType> = UmbWorkspaceActionDefaultKind<MetaType>,
> extends UmbLitElement {
	#manifest?: ManifestWorkspaceAction<MetaType>;
	#api?: ApiType;
	protected _extensionsController?: UmbExtensionsElementAndApiInitializer<
		ManifestWorkspaceActionMenuItem,
		'workspaceActionMenuItem',
		ManifestWorkspaceActionMenuItem
	>;

	@property({ type: Object, attribute: false })
	public set manifest(value: ManifestWorkspaceAction<MetaType> | undefined) {
		if (!value) return;
		const oldValue = this.#manifest;
		if (oldValue !== value) {
			this.#manifest = value;
			this._href = value?.meta.href;
			this._additionalOptions = value?.meta.additionalOptions;
			this.#createAliases();
		}
	}
	public get manifest() {
		return this.#manifest;
	}

	@property({ attribute: false })
	public set api(api: ApiType | undefined) {
		this.#api = api;

		this.#api?.getHref?.().then((href) => {
			this._href = href ?? this.manifest?.meta.href;
		});

		this.#api?.hasAdditionalOptions?.().then((additionalOptions) => {
			this._additionalOptions = additionalOptions ?? this.manifest?.meta.additionalOptions;
		});

		this.#observeIsDisabled();
	}
	public get api(): ApiType | undefined {
		return this.#api;
	}

	protected _actionApi?: UmbAction<UmbWorkspaceActionArgs<MetaType>>;

	protected _buttonLabel?: string;

	@state()
	private _buttonState?: UUIButtonState;

	@state()
	private _additionalOptions?: boolean;

	@state()
	private _href?: string;

	@state()
	private _isDisabled = false;

	@state()
	protected _items: Array<UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>> = [];

	#buttonStateResetTimeoutId: number | null = null;

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
				const overwrites = Array.isArray(this.#manifest.overwrites)
					? this.#manifest.overwrites
					: [this.#manifest.overwrites];
				for (const alias of overwrites) {
					aliases.add(alias);
				}
			}
		}

		this.observeExtensions(Array.from(aliases));
	}

	async #onClick(event: MouseEvent) {
		if (this._href) {
			event.stopPropagation();
		}
		// If its a link or has additional options, then we do not want to display state on the button. [NL]
		if (!this._href) {
			if (!this._additionalOptions) {
				this._buttonState = 'waiting';
			}

			try {
				const api = this._actionApi ?? this.#api;
				if (!api) throw new Error('No api defined');
				await api.execute();
				this._buttonState = 'success';
				this.#initButtonStateReset();
			} catch (reason) {
				if (reason) {
					console.warn(reason);
				}
				this._buttonState = 'failed';
				this.#initButtonStateReset();
			}
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

	#initButtonStateReset() {
		/* When the button has additional options, we do not show the waiting state.
    Therefore, we need to ensure the button state is reset, so we are able to show the success state again. */
		this.#clearButtonStateResetTimeout();

		this.#buttonStateResetTimeoutId = window.setTimeout(() => {
			this._buttonState = undefined;
		}, 2000);
	}

	#clearButtonStateResetTimeout() {
		if (this.#buttonStateResetTimeoutId !== null) {
			clearTimeout(this.#buttonStateResetTimeoutId);
			this.#buttonStateResetTimeoutId = null;
		}
	}

	protected observeExtensions(aliases: string[]): void {
		this._extensionsController?.destroy();
		this._extensionsController = new UmbExtensionsElementAndApiInitializer<
			ManifestWorkspaceActionMenuItem,
			'workspaceActionMenuItem',
			ManifestWorkspaceActionMenuItem
		>(
			this,
			umbExtensionsRegistry,
			'workspaceActionMenuItem',
			(manifest) => [{ meta: manifest.meta }],
			(action) => stringOrStringArrayIntersects(action.forWorkspaceActions, aliases),
			(actions) => {
				this._items = actions;
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
		);
	}

	#renderButton() {
		const label = this.localize.string(this._buttonLabel || this.#manifest?.meta.label || this.#manifest?.name || '');
		return html`
			<uui-button
				data-mark="workspace-action:${this.#manifest?.alias}"
				color=${this.#manifest?.meta.color ?? 'default'}
				look=${this.#manifest?.meta.look ?? 'default'}
				label=${this._additionalOptions ? label + '…' : label}
				.disabled=${this._isDisabled}
				.href=${this._href}
				.state=${this._buttonState}
				@click=${this.#onClick}></uui-button>
		`;
	}

	#renderActionMenu() {
		return html`
			<umb-workspace-action-menu
				color=${this.#manifest?.meta.color ?? 'default'}
				look=${this.#manifest?.meta.look ?? 'default'}
				.items=${this._items}></umb-workspace-action-menu>
		`;
	}

	override render() {
		return when(
			this._items.length,
			() => html`<uui-button-group>${this.#renderButton()}${this.#renderActionMenu()}</uui-button-group>`,
			() => this.#renderButton(),
		);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#clearButtonStateResetTimeout();
	}
}

export default UmbWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action': UmbWorkspaceActionElement;
	}
}
