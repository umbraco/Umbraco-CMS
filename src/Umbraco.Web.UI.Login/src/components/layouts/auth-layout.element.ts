import { css, CSSResultGroup, html, LitElement, nothing, PropertyValueMap } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';

/**
 * The auth layout component.
 *
 * @element umb-auth-layout
 * @slot - The content of the layout
 * @cssprop --umb-login-background - The background of the layout (default: #f4f4f4)
 * @cssprop --umb-login-primary-color - The color of the headline (default: #283a97)
 * @cssprop --umb-login-text-color - The color of the text (default: #000)
 * @cssprop --umb-login-header-font-size - The font-size of the headline (default: 3rem)
 * @cssprop --umb-login-header-font-size-large - The font-size of the headline on large screens (default: 4rem)
 * @cssprop --umb-login-header-secondary-font-size - The font-size of the secondary headline (default: 2.4rem)
 * @cssprop --umb-login-image - The background of the image wrapper (default: the value of the backgroundImage property)
 * @cssprop --umb-login-image-display - The display of the image wrapper (default: flex)
 * @cssprop --umb-login-image-border-radius - The border-radius of the image wrapper (default: 38px)
 * @cssprop --umb-login-content-background - The background of the content wrapper (default: none)
 * @cssprop --umb-login-content-display - The display of the content wrapper (default: flex)
 * @cssprop --umb-login-content-width - The width of the content wrapper (default: 100%)
 * @cssprop --umb-login-content-height - The height of the content wrapper (default: 100%)
 * @cssprop --umb-login-content-border-radius - The border-radius of the content wrapper (default: 0)
 * @cssprop --umb-login-align-items - The align-items of the main wrapper (default: unset)
 * @cssprop --umb-login-button-border-radius - The border-radius of the buttons (default: 45px)
 * @cssprop --umb-login-curves-color - The color of the curves (default: #f5c1bc)
 * @cssprop --umb-login-curves-display - The display of the curves (default: inline)
 */
@customElement('umb-auth-layout')
export class UmbAuthLayoutElement extends LitElement {
  @property({ attribute: 'background-image' })
  backgroundImage?: string;

  @property({ attribute: 'logo-image' })
  logoImage?: string;

  @property({ attribute: 'logo-image-alternative' })
  logoImageAlternative?: string;

  protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
    super.updated(_changedProperties);

    if (_changedProperties.has<keyof this>('backgroundImage')) {
      this.style.setProperty('--logo-alternative-display', this.backgroundImage ? 'none' : 'unset');
      this.style.setProperty('--image', `url('${this.backgroundImage}') no-repeat center center/cover`);
    }
  }

  #renderImageContainer() {
    if (!this.backgroundImage) return nothing;

    return html`
      <div id="image-container">
        <div id="image">
          <svg
            id="curve-top"
            width="1746"
            height="1374"
            viewBox="0 0 1746 1374"
            fill="none"
            xmlns="http://www.w3.org/2000/svg">
            <path d="M8 1C61.5 722.5 206.5 1366.5 1745.5 1366.5" stroke="currentColor" stroke-width="15"/>
          </svg>
          <svg
            id="curve-bottom"
            width="1364"
            height="552"
            viewBox="0 0 1364 552"
            fill="none"
            xmlns="http://www.w3.org/2000/svg">
            <path d="M1 8C387 24 1109 11 1357 548" stroke="currentColor" stroke-width="15"/>
          </svg>

          ${when(
            this.logoImage,
            () => html`<img id="logo-on-image" src=${this.logoImage} alt="logo" aria-hidden="true"/>`
          )}
        </div>
      </div>
    `;
  }

  #renderContent() {
    return html`
      <div id="content-container">
        <div id="content">
          <slot></slot>
        </div>
      </div>
    `;
  }

  render() {
    return html`
      <div id=${this.backgroundImage ? 'main' : 'main-no-image'}>
        ${this.#renderImageContainer()} ${this.#renderContent()}
      </div>
      ${when(
        this.logoImageAlternative,
        () => html`<img id="logo-on-background" src=${this.logoImageAlternative!} alt="logo" aria-hidden="true"/>`
      )}
    `;
  }

  static styles: CSSResultGroup = [
    css`
      :host {
        --uui-color-interactive: var(--umb-login-primary-color, #283a97);
        --uui-button-border-radius: var(--umb-login-button-border-radius, 45px);
        --uui-color-default: var(--uui-color-interactive);
        --uui-button-height: 42px;
        --uui-select-height: 38px;

        --input-height: 40px;
        --header-font-size: var(--umb-login-header-font-size, 3rem);
        --header-secondary-font-size: var(--umb-login-header-secondary-font-size, 2.4rem);
        --curves-color: var(--umb-login-curves-color, #f5c1bc);
        --curves-display: var(--umb-login-curves-display, inline);

        display: block;
        background: var(--umb-login-background, #f4f4f4);
        color: var(--umb-login-text-color, #000);
      }

      #main-no-image,
      #main {
        max-width: 1920px;
        display: flex;
        justify-content: center;
        height: 100vh;
        padding: 8px;
        box-sizing: border-box;
        margin: 0 auto;
      }

      #image-container {
        display: var(--umb-login-image-display, none);
        width: 100%;
      }

      #content-container {
        background: var(--umb-login-content-background, none);
        display: var(--umb-login-content-display, flex);
        width: var(--umb-login-content-width, 100%);
        height: var(--umb-login-content-height, 100%);
        box-sizing: border-box;
        overflow: auto;
        border-radius: var(--umb-login-content-border-radius, 0);
      }

      #content {
        max-width: 360px;
        margin: auto;
        width: 100%;
      }

      #image {
        background: var(--umb-login-image, var(--image));
        width: 100%;
        height: 100%;
        border-radius: var(--umb-login-image-border-radius, 38px);
        position: relative;
        overflow: hidden;
        color: var(--curves-color);
      }

      #image svg {
        position: absolute;
        width: 45%;
        height: fit-content;
        display: var(--curves-display);
      }

      #curve-top {
        top: 0;
        right: 0;
      }

      #curve-bottom {
        bottom: 0;
        left: 0;
      }

      #logo-on-image,
      #logo-on-background {
        position: absolute;
        top: 24px;
        left: 24px;
        height: 55px;
      }

      @media only screen and (min-width: 900px) {
        :host {
          --header-font-size: var(--umb-login-header-font-size-large, 4rem);
        }

        #main {
          padding: 32px;
          padding-right: 0;
          align-items: var(--umb-login-align-items, unset);
        }

        #image-container {
          display: var(--umb-login-image-display, block);
        }

        #content-container {
          display: var(--umb-login-content-display, flex);
          padding: 16px;
        }

        #logo-on-background {
          display: var(--logo-alternative-display);
        }
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-auth-layout': UmbAuthLayoutElement;
  }
}
