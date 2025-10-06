import type { ManifestPreviewOption } from '../preview-option/preview-option.extension.js';
import { customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-save-and-preview-workspace-action')
export class UmbSaveAndPreviewWorkspaceActionElement extends UmbLitElement {
	#buttonStateResetTimeoutId: number | null = null;

	#extensionsController?: UmbExtensionsElementAndApiInitializer<
		ManifestPreviewOption,
		'previewOption',
		ManifestPreviewOption
	>;

	@property({ type: Object, attribute: false })
	public set manifest(value: ManifestPreviewOption | undefined) {
		if (!value) return;
		const oldValue = this.#manifest;
		if (oldValue !== value) {
			this.#manifest = value;
			this.#observeExtensions();
		}
	}
	public get manifest() {
		return this.#manifest;
	}
	#manifest?: ManifestPreviewOption;

	@property({ attribute: false })
	public set api(api: UmbWorkspaceAction | undefined) {
		this.#api = api;
		this.#observeIsDisabled();
	}
	public get api(): UmbWorkspaceAction | undefined {
		return this.#api;
	}
	#api?: UmbWorkspaceAction;

	@state()
	private _buttonState?: UUIButtonState;

	@state()
	private _isDisabled = false;

	@state()
	private _actions: Array<UmbExtensionElementAndApiInitializer<ManifestPreviewOption>> = [];

	private _primaryAction?: UmbExtensionElementAndApiInitializer<ManifestPreviewOption>;

	async #onClick() {
		this._buttonState = 'waiting';

		try {
			if (!this._primaryAction?.api) throw new Error('No api defined');
			await this._primaryAction.api.execute().catch(() => {});
			this._buttonState = 'success';
		} catch (reason) {
			if (reason) {
				console.warn(reason);
			}
			this._buttonState = 'failed';
		}

		this.#initButtonStateReset();
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

	#observeExtensions(): void {
		this.#extensionsController?.destroy();
		this.#extensionsController = new UmbExtensionsElementAndApiInitializer<
			ManifestPreviewOption,
			'previewOption',
			ManifestPreviewOption
		>(this, umbExtensionsRegistry, 'previewOption', [], undefined, (extensionControllers) => {
			this._primaryAction = extensionControllers.shift();
			this._actions = extensionControllers;
		});
	}

	#renderButton() {
		const label = this._primaryAction?.manifest?.meta.label || this.#manifest?.meta.label || this.#manifest?.name;
		return html`
			<uui-button
				data-mark="workspace-action:${this.#manifest?.alias}"
				color=${this.#manifest?.meta.color ?? 'default'}
				look=${this.#manifest?.meta.look ?? 'default'}
				label=${this.localize.string(label)}
				.disabled=${this._isDisabled}
				.state=${this._buttonState}
				@click=${this.#onClick}></uui-button>
		`;
	}

	#renderActionMenu() {
		// TODO: [LK] FIXME: The `any` type casting here needs to be resolved with a proper type.
		return html`
			<umb-workspace-action-menu
				color=${this.#manifest?.meta.color ?? 'default'}
				look=${this.#manifest?.meta.look ?? 'default'}
				.items=${this._actions as any}></umb-workspace-action-menu>
		`;
	}

	override render() {
		return when(
			this._actions.length,
			() => html`<uui-button-group>${this.#renderButton()}${this.#renderActionMenu()}</uui-button-group>`,
			() => this.#renderButton(),
		);
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#clearButtonStateResetTimeout();
	}
}

export default UmbSaveAndPreviewWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-save-and-preview-workspace-action': UmbSaveAndPreviewWorkspaceActionElement;
	}
}
