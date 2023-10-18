import { html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { unsafeHTML } from 'lit/directives/unsafe-html.js';
import { umbLocalizationContext } from './localization-context.js';

/**
 * The localize element.
 * It takes a key and returns the localized value for that key.
 * If the key is not found, it uses the default slot as fallback.
 *
 * @slot - The default slot should contain the fallback text.
 * @element umb-localize
 * @example <umb-localize key="login_loginButton">Login</umb-localize>
 */
@customElement('umb-localize')
export class UmbLocalizeElement extends LitElement {
	@property({ type: String })
	key!: string;

	@state()
	value = '';

	connectedCallback() {
		super.connectedCallback();
		this.#load();
	}

	async #load() {
		try {
			this.value = await this.localize(this.key);
		} catch (error: any) {
			console.error('Failed to localize key:', this.key, error);
		}
	}

	async localize(key: string, fallback?: string): Promise<string> {
		return umbLocalizationContext.localize(key, undefined, fallback);
	}

	render() {
		return this.value ? html`${unsafeHTML(this.value)}` : html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-localize': UmbLocalizeElement;
	}
}
