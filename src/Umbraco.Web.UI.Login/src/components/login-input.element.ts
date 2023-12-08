import { UUIInputElement } from '@umbraco-ui/uui';
import { customElement } from 'lit/decorators.js';

/**
 * This is a custom element based on UUIInputElement that is used in the login page.
 * It differs from UUIInputElement in that it does not render a Shadow DOM.
 *
 * @element umb-login-input
 * @inheritDoc UUIInputElement
 */
@customElement('umb-login-input')
export class UmbLoginInputElement extends UUIInputElement {
  /**
   * Remove the id attribute from the inner input element to avoid duplicate ids.
   *
   * @override
   * @protected
   */
  protected firstUpdated() {
    const innerInput = this.querySelector('input');
    innerInput?.removeAttribute('id');

    innerInput?.addEventListener('mousedown', () => {
      this.style.setProperty('--uui-show-focus-outline', '0');
    });
    innerInput?.addEventListener('blur', () => {
      this.style.setProperty('--uui-show-focus-outline', '');
    });
  }

  /**
   * Since this element does not render a Shadow DOM nor does it have a unique ID,
   * we need to override this method to get the form element.
   *
   * @override
   * @protected
   */
  protected getFormElement(): HTMLElement {
    const formElement = this.querySelector('input');

    if (!formElement) {
      throw new Error('Form element not found');
    }

    return formElement;
  }

  /**
   * Instruct Lit to not render a Shadow DOM.
   *
   * @protected
   */
  protected createRenderRoot() {
    return this;
  }

  static styles = [...UUIInputElement.styles];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-login-input': UmbLoginInputElement;
  }
}
