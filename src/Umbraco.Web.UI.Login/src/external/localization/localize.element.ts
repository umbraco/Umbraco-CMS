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
  #key = '';

	@property({ type: String })
	set key (value: string) {
    this.#key = value;
    this.#text().then((val) => {
      this.value = val;
    });
  }
  get key() {
    return this.#key;
  }

  get hasFallbackValue() {
    return !!this.textContent;
  }

	@state()
	value = '';

  async #text(): Promise<string> {
    const localizedValue = await this.localize(this.key);

    // If the value is the same as the key, it means the key was not found.
    if (localizedValue === '#fallback#') {
      return this.textContent ?? this.key;
    }

    return localizedValue;
  }

	async localize(key: string): Promise<string> {
		return umbLocalizationContext.localize(key, undefined, '#fallback#');
	}

  render() {
    return html`${unsafeHTML(this.value)}`
  }
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-localize': UmbLocalizeElement;
	}
}
