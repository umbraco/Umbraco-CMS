import type { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui';
import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { until } from 'lit/directives/until.js';

import { loadCustomView, renderCustomView } from '../utils/load-custom-view.function.js';
import { umbLocalizationContext } from '../external/localization/localization-context.js';

type UserViewState = 'loggingIn' | 'loggedIn' | 'loggedOut' | 'timedOut';

type ExternalLoginCustomViewElement = HTMLElement & {
  displayName?: string;
  providerName?: string;
  externalLoginUrl?: string;
  userViewState?: UserViewState;
};

/**
 * This elements represents a single external login provider and should be slotted into the <umb-auth> element.
 *
 * @element umb-external-login-provider
 */
@customElement('umb-external-login-provider')
export class UmbExternalLoginProviderElement extends LitElement {
  /**
   * Gets or sets the path to the module that should be loaded as the custom view.
   * The module should export a default class that extends HTMLElement.
   *
   * Setting this property will cause the default view to be hidden and the custom view to be loaded.
   * The icon, button look and button color will be ignored.
   *
   * @example App_Plugins/MyPackage/MyCustomLoginView.js
   * @attr custom-view
   */
  @property({attribute: 'custom-view'})
  customView?: string;

  /**
   * Gets or sets the display name of the provider.
   *
   * @attr display-name
   * @example Google
   */
  @property({attribute: 'display-name'})
  displayName = '';

  /**
   * Gets or sets the name of the provider (otherwise known as authentication type).
   *
   * @attr provider-name
   * @example Umbraco.Google
   */
  @property({attribute: 'provider-name'})
  providerName = '';

  /**
   * Gets or sets the view state of the user. This indicates in which state the user is in the login process,
   * which can be used to determine where the external-login-provider is being shown.
   *
   * @attr user-view-state
   * @example loggingIn
   * @default loggingIn
   */
  @property({attribute: 'user-view-state'})
  userViewState: UserViewState = 'loggingIn';

  /**
   * Gets or sets the url to the external login provider.
   *
   * @attr external-login-url
   * @example /umbraco/ExternalLogin
   */
  @property({attribute: 'external-login-url'})
  set externalLoginUrl(value: string) {
    const tempUrl = new URL(value, window.location.origin);
    const searchParams = new URLSearchParams(tempUrl.search);
    tempUrl.searchParams.append('redirectUrl', decodeURIComponent(searchParams.get('returnPath') ?? ''));
    this.#externalLoginUrl = tempUrl.pathname + tempUrl.search;
  }

  get externalLoginUrl() {
    return this.#externalLoginUrl;
  }

  /**
   * Gets or sets the icon to display next to the provider name.
   * This should be the name of an icon in the Umbraco Backoffice icon set.
   *
   * @attr icon
   * @example icon-google-fill
   * @default icon-lock
   */
  @property({attribute: 'icon'})
  icon = 'icon-lock';

  /**
   * Gets or sets the look of the underlying uui-button.
   *
   * @attr button-look
   * @example outline
   * @default outline
   * @see https://uui.umbraco.com/?path=/story/uui-button--looks-and-colors
   */
  @property({attribute: 'button-look'})
  buttonLook: InterfaceLook = 'outline';

  /**
   * Gets or sets the color of the underlying uui-button.
   *
   * @attr button-color
   * @example danger
   * @default default
   * @see https://uui.umbraco.com/?path=/story/uui-button--looks-and-colors
   */
  @property({attribute: 'button-color'})
  buttonColor: InterfaceColor = 'default';

  #externalLoginUrl = '';

  constructor() {
    super();

    const searchParams = new URLSearchParams(window.location.search);
    const isLogout = searchParams.get('logout') === 'true';

    if (isLogout) {
      this.userViewState = 'loggedOut';
    }
  }

  protected render() {
    return this.customView
      ? until(this.renderCustomView(), html`
        <uui-loader-bar></uui-loader-bar>`)
      : this.renderDefaultView();
  }

  protected renderDefaultView() {
    return html`
      <form id="defaultView" method="post" action=${this.externalLoginUrl}>
        <uui-button
          type="submit"
          name="provider"
          .value=${this.providerName}
          .label=${until(umbLocalizationContext.localize('login_signInWith', undefined, 'Sign in with').then(str => `${str} ${this.displayName}`))}
          .look=${this.buttonLook}
          .color=${this.buttonColor}>
          ${this.displayName
            ? html`
              <div>
                <uui-icon name=${this.icon}></uui-icon>
                <umb-localize key="login_signInWith">Sign in with</umb-localize>
                ${this.displayName}
              </div>
            `
            : nothing}
          <slot></slot>
        </uui-button>
      </form>
    `;
  }

  protected async renderCustomView() {
    try {
      if (!this.customView) return;

      const customView = await loadCustomView<ExternalLoginCustomViewElement>(this.customView);

      if (typeof customView === 'object') {
        customView.displayName = this.displayName;
        customView.providerName = this.providerName;
        customView.externalLoginUrl = this.externalLoginUrl;
        customView.userViewState = this.userViewState;
      }

      return renderCustomView(customView);
    } catch (error: unknown) {
      console.group('[External login] Failed to load custom view');
      console.log('Provider name', this.providerName);
      console.log('Element reference', this);
      console.log('Custom view', this.customView);
      console.error('Failed to load custom view:', error);
      console.groupEnd();
    }
  }

  static styles: CSSResultGroup = [
    css`
      #defaultView uui-button {
        width: 100%;
        --uui-button-font-weight: 400;
      }

      #defaultView uui-button div {
        /* TODO: Remove this when uui-button has setting for aligning content */
        position: absolute;
        top: 50%;
        left: 0;
        margin: auto;
        transform: translateY(-50%);
        text-align: left;
        padding-left: 15px;
      }

      #defaultView uui-icon {
        color: #00000080;
        padding-right: 2px;
      }

      #defaultView button {
        font-size: var(--uui-button-font-size);
        border: 1px solid var(--uui-color-border);
        border-radius: var(--uui-button-border-radius);
        width: 100%;
        padding: 9px;
        text-align: left;
        background-color: var(--uui-color-surface);
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-2);
        box-sizing: border-box;

        line-height: 1.1; /* makes the text vertically centered */
        color: var(--uui-color-interactive);
      }

      #defaultView button:hover {
        color: var(--uui-color-interactive-emphasis);
        border-color: var(--uui-color-border-standalone);
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-external-login-provider': UmbExternalLoginProviderElement;
  }
}
