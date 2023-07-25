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
	@property({ type: String })
	key!: string;

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
		try {
			if (this.#subscription) {
				this.#subscription.destroy();
			}

			this.#subscription = this.observe(localizationContext!.localize(this.key), (value) => {
				this.value = value;
			});
		} catch (error: any) {
			console.error('Failed to localize key:', this.key, error);
		}
	}

	render() {
		return this.value ? html`${this.value}` : html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-localize': UmbLocalizeElement;
	}
}
