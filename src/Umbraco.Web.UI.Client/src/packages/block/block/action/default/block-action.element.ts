import type { ManifestBlockAction } from '../block-action.extension.js';
import type { UmbBlockAction } from '../block-action.interface.js';
import type { UmbBlockActionElement } from '../block-action-element.interface.js';
import type { MetaBlockActionDefaultKind } from './types.js';
import { css, customElement, html, ifDefined, property, state, when } from '@umbraco-cms/backoffice/external/lit';
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
		this._href = undefined;

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
		if (!this.manifest) return;
		const label = this.manifest.meta.label ? this.localize.string(this.manifest.meta.label) : this.manifest.name;
		return html`
			<uui-button
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

	static override styles = [
		/* uui-action-bar sets --uui-button-* custom properties on the slotted <umb-block-action>
		   via ::slotted(*:first-child) etc. Those values inherit down, but uui-button's own
		   :host declarations redeclare them on its element, shadowing the inherited values.
		   To pass them through, we bridge via intermediate property names on :host (breaking
		   the self-reference cycle) and apply those on the inner uui-button. */
		css`
			:host {
				--umb-button-border-radius: var(--uui-button-border-radius);
				--umb-button-padding-left-factor: var(--uui-button-padding-left-factor);
				--umb-button-padding-right-factor: var(--uui-button-padding-right-factor);
			}

			uui-button {
				--uui-button-border-radius: var(--umb-button-border-radius);
				--uui-button-padding-left-factor: var(--umb-button-padding-left-factor);
				--uui-button-padding-right-factor: var(--umb-button-padding-right-factor);
			}
		`,
	];
}

export default UmbBlockActionDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-action': UmbBlockActionDefaultElement;
	}
}
