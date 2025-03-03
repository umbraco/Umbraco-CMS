import type {
	ManifestWorkspaceAction,
	ManifestWorkspaceActionMenuItem,
	MetaWorkspaceActionDefaultKind,
	UmbWorkspaceActionDefaultKind,
} from '../../../types.js';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	type UmbExtensionElementAndApiInitializer,
	UmbExtensionsElementAndApiInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { stringOrStringArrayIntersects } from '@umbraco-cms/backoffice/utils';

import '../../workspace-action-menu/index.js';

@customElement('umb-workspace-action')
export class UmbWorkspaceActionElement<
	MetaType extends MetaWorkspaceActionDefaultKind = MetaWorkspaceActionDefaultKind,
	ApiType extends UmbWorkspaceActionDefaultKind<MetaType> = UmbWorkspaceActionDefaultKind<MetaType>,
> extends UmbLitElement {
	#manifest?: ManifestWorkspaceAction<MetaType>;
	#api?: ApiType;
	#extensionsController?: UmbExtensionsElementAndApiInitializer<
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

	@state()
	private _buttonState?: UUIButtonState;

	@state()
	private _additionalOptions?: boolean;

	@state()
	private _href?: string;

	@state()
	_isDisabled = false;

	@state()
	private _items: Array<UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>> = [];

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

		this.#observeExtensions(Array.from(aliases));
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
				if (!this.#api) throw new Error('No api defined');
				await this.#api.execute();
				if (!this._additionalOptions) {
					this._buttonState = 'success';
				}
			} catch {
				if (!this._additionalOptions) {
					this._buttonState = 'failed';
				}
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

	#observeExtensions(aliases: string[]): void {
		this.#extensionsController?.destroy();
		this.#extensionsController = new UmbExtensionsElementAndApiInitializer<
			ManifestWorkspaceActionMenuItem,
			'workspaceActionMenuItem',
			ManifestWorkspaceActionMenuItem
		>(
			this,
			umbExtensionsRegistry,
			'workspaceActionMenuItem',
			ExtensionApiArgsMethod,
			(action) => stringOrStringArrayIntersects(action.forWorkspaceActions, aliases),
			(extensionControllers) => {
				this._items = extensionControllers;
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
		);
	}

	#renderButton() {
		const label = this.#manifest?.meta.label
			? this.localize.string(this.#manifest.meta.label)
			: (this.#manifest?.name ?? '');
		return html`
			<uui-button
				data-mark="workspace-action:${this.#manifest?.alias}"
				.href=${this._href}
				look=${this.#manifest?.meta.look ?? 'default'}
				color=${this.#manifest?.meta.color ?? 'default'}
				label=${this._additionalOptions ? label + '…' : label}
				.disabled=${this._isDisabled}
				.state=${this._buttonState}
				@click=${this.#onClick}></uui-button>
		`;
	}

	#renderActionMenu() {
		return html`
			<umb-workspace-action-menu
				.items=${this._items}
				color="${this.#manifest?.meta.color ?? 'default'}"
				look="${this.#manifest?.meta.look ?? 'default'}"></umb-workspace-action-menu>
		`;
	}

	override render() {
		return when(
			this._items.length,
			() => html` <uui-button-group> ${this.#renderButton()} ${this.#renderActionMenu()} </uui-button-group> `,
			() => this.#renderButton(),
		);
	}
}

export default UmbWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action': UmbWorkspaceActionElement;
	}
}

/**
 *
 * @param manifest
 * @returns An array of arguments to pass to the extension API initializer.
 */
function ExtensionApiArgsMethod(manifest: ManifestWorkspaceActionMenuItem) {
	return [{ meta: manifest.meta }];
}
