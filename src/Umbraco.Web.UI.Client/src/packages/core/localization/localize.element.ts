import { UMB_LOCALIZATION_CONTEXT } from '@umbraco-cms/backoffice/localization-api';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

/**
 * This element allows you to localize a string with optional interpolation values.
 * @element umb-localize
 */
@customElement('umb-localize')
export class UmbLocalizeElement extends UmbLitElement {
	/**
	 * The key to localize. The key is case sensitive.
	 * @attr
	 * @example key="general_ok"
	 */
	@property({ type: String })
	set key(value: string) {
		const isNewKey = this.#key !== value;
		this.#key = value;

		// Only reload translations if the key has changed, otherwise the load happens when the context is there.
		if (isNewKey) {
			this.#load();
		}
	}

	get key() {
		return this.#key;
	}

	/**
	 * If true, the key will be rendered instead of the localized value if the key is not found.
	 * @attr
	 */
	@property({ type: Boolean })
	debug = false;

	@state()
	protected value?: string;

	#key = '';
	#localizationContext?: typeof UMB_LOCALIZATION_CONTEXT.TYPE;
	#subscription?: UmbObserverController<string>;

	constructor() {
		super();
		this.consumeContext(UMB_LOCALIZATION_CONTEXT, (instance) => {
			this.#localizationContext = instance;
			this.#load();
		});
	}

	async #load() {
		if (this.#subscription) {
			this.#subscription.destroy();
		}

		if (!this.#localizationContext) {
			return;
		}

		this.#subscription = this.observe(this.#localizationContext!.localize(this.key), (value) => {
			if (value) {
				(this.getHostElement() as HTMLElement).removeAttribute('data-umb-localize-error');
				this.value = value;
			} else {
				(this.getHostElement() as HTMLElement).setAttribute('data-umb-localize-error', `Key not found: ${this.key}`);
				console.warn('Key not found:', this.key, this);
				if (this.debug) {
					this.value = this.key;
				}
			}
		});
	}

	protected render() {
		return this.value ? html`${this.value}` : html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-localize': UmbLocalizeElement;
	}
}
