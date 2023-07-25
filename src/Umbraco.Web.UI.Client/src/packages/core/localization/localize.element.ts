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
	key!: string;

	/**
	 * If true, the key will be rendered instead of the localized value if the key is not found.
	 * @attr
	 */
	@property({ type: Boolean })
	debug = false;

	@state()
	get value(): string {
		return this.#value;
	}

	set value(value: string) {
		const oldValue = this.#value;
		this.#value = value;
		this.requestUpdate('value', oldValue);
	}

	#value: string = '';
	#subscription?: UmbObserverController<string>;

	constructor() {
		super();
		this.consumeContext(UMB_LOCALIZATION_CONTEXT, (instance) => {
			this.#load(instance);
		});
	}

	async #load(localizationContext: typeof UMB_LOCALIZATION_CONTEXT.TYPE) {
		if (this.#subscription) {
			this.#subscription.destroy();
		}

		this.#subscription = this.observe(localizationContext!.localize(this.key), (value) => {
			if (value) {
				(this.getHostElement() as HTMLElement).removeAttribute('data-umb-localize-error');
				this.value = value;
			} else {
				(this.getHostElement() as HTMLElement).setAttribute('data-umb-localize-error', `Key not found: ${this.key}`);
				console.warn('Key not found:', this.key);
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
