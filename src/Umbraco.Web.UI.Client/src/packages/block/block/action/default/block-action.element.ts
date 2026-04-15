import type { ManifestBlockAction } from '../block-action.extension.js';
import type { UmbBlockAction } from '../block-action.interface.js';
import type { UmbBlockActionElement } from '../block-action-element.interface.js';
import type { MetaBlockActionDefaultKind } from './types.js';
import { customElement, html, ifDefined, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbObserveValidationStateController } from '@umbraco-cms/backoffice/validation';

@customElement('umb-block-action')
export class UmbBlockActionDefaultElement<
		MetaType extends MetaBlockActionDefaultKind = MetaBlockActionDefaultKind,
		ApiType extends UmbBlockAction<MetaType> = UmbBlockAction<MetaType>,
	>
	extends UmbLitElement
	implements UmbBlockActionElement
{
	#api?: ApiType;

	@property({ attribute: false })
	public manifest?: ManifestBlockAction<MetaType>;

	public set api(api: ApiType | undefined) {
		this.#api = api;

		// TODO: getHref() and getValidationDataPath() resolve once. If the underlying observable values
		// change, the button won't update. Consider making these reactive in a future iteration. [LK]
		this.#api?.getHref?.().then((href) => {
			this._href = href;
		});

		this.#api?.getValidationDataPath?.().then((path) => {
			this.removeUmbControllerByAlias('observeValidation');
			if (path) {
				new UmbObserveValidationStateController(
					this,
					path,
					(hasMessages) => (this._invalid = hasMessages),
					'observeValidation',
				);
			}
		});
	}

	@state()
	private _href?: string;

	@state()
	private _invalid = false;

	async #onClick(event: PointerEvent) {
		if (this._href) return;

		event.stopPropagation();

		try {
			await this.#api?.execute();
			this.dispatchEvent(new UmbActionExecutedEvent());
		} catch (error) {
			console.error('Error executing block action:', error);
		}
	}

	override render() {
		if (!this.manifest) return nothing;
		const label = this.manifest.meta.label ? this.localize.string(this.manifest.meta.label) : this.manifest.name;
		return html`
			<uui-button
				compact
				data-mark="block-action:${this.manifest.alias}"
				href=${ifDefined(this._href)}
				look="secondary"
				color=${this._invalid ? 'invalid' : ''}
				label=${label}
				title=${label}
				@click=${this.#onClick}>
				${when(this.manifest.meta.icon, (icon) => html`<uui-icon name=${icon}></uui-icon>`)}
				${when(this._invalid, () => html`<uui-badge attention color="invalid" label="Invalid">!</uui-badge>`)}
			</uui-button>
		`;
	}
}

export default UmbBlockActionDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-action': UmbBlockActionDefaultElement;
	}
}
