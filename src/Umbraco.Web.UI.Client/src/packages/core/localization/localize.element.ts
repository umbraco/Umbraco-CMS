import { UMB_LOCALIZATION_CONTEXT } from './localization.context.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { Subscription } from '@umbraco-cms/backoffice/external/rxjs';

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
	#localizationContext?: typeof UMB_LOCALIZATION_CONTEXT.TYPE;
	#subscription?: Subscription;

	constructor() {
		super();
		this.consumeContext(UMB_LOCALIZATION_CONTEXT, (instance) => {
			this.#localizationContext = instance;
			this.#load();
		});
	}

	connectedCallback() {
		super.connectedCallback();
		this.#load();
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		if (this.#subscription) {
			this.#subscription.unsubscribe();
		}
	}

	async #load() {
		try {
			if (this.#subscription) {
				this.#subscription.unsubscribe();
			}

			this.#subscription = this.#localizationContext!.localize(this.key).subscribe((value) => {
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
