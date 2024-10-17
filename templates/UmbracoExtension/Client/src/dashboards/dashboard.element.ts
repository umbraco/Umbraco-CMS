import { LitElement, css, html, customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { ExamplesService, UserModel } from "../api";
import { UUIButtonElement } from "@umbraco-cms/backoffice/external/uui";
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from "@umbraco-cms/backoffice/notification";
import { UMB_CURRENT_USER_CONTEXT, UmbCurrentUserContext, UmbCurrentUserModel } from "@umbraco-cms/backoffice/current-user";

@customElement('my-awesome-dashboard')
export class MyAwesomeDashboardElement extends UmbElementMixin(LitElement) {

  constructor() {
    super();

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
      this.#notificationContext = notificationContext;
    });

    this.consumeContext(UMB_CURRENT_USER_CONTEXT, (currentUserContext) => {

      // When we have the current user context
      // We can observe proeprties from it, such as the current user or perhaps just individual properties
      // When the currentUser object changes we will get notified and can reset the @state properrty
      this.observe(currentUserContext.currentUser, (currentUser) => {
        this._contextCurrentUser = currentUser;
      });
    });
  }

  #notificationContext: UmbNotificationContext | undefined = undefined;

  #onClickWhoAmI = async (ev: Event) => {
    const buttonElement = ev.target as UUIButtonElement;
    buttonElement.state = "waiting";

    const { data, error } = await ExamplesService.getUmbracoExampleApiV1WhoAmI();

    if (error) {
      buttonElement.state = "failed";
      console.error(error);
      return;
    }

    if (data !== undefined) {
      this._serverUserData = data;
      buttonElement.state = "success";
    }

    if (this.#notificationContext) {
      this.#notificationContext.peek("warning", {
        data: {
          headline: `You are ${this._serverUserData?.name}`,
          message: `Your email is ${this._serverUserData?.email}`,
        }
      })
    }
  }

  #onClickWhatsTheTimeMrWolf = async (ev: Event) => {
    const buttonElement = ev.target as UUIButtonElement;
    buttonElement.state = "waiting";

    // Getting a string - should I expect a datetime?!
    const { data, error } = await ExamplesService.getUmbracoExampleApiV1WhatsTheTimeMrWolf();

    if (error) {
      buttonElement.state = "failed";
      console.error(error);
      return;
    }

    if (data !== undefined) {
      this._timeFromMrWolf = new Date(data);
      buttonElement.state = "success";
    }
  }

  #onClickWhatsMyName = async (ev: Event) => {
    const buttonElement = ev.target as UUIButtonElement;
    buttonElement.state = "waiting";

    const { data, error } = await ExamplesService.getUmbracoExampleApiV1WhatsMyName();

    if (error) {
      buttonElement.state = "failed";
      console.error(error);
      return;
    }

    this._yourName = data;
    buttonElement.state = "success";
  }

  @state()
  private _yourName: string | undefined = "Dunno";

  @state()
  private _timeFromMrWolf: Date | undefined;

  @state()
  private _serverUserData: UserModel | undefined = undefined;

  @state()
  private _contextCurrentUser: UmbCurrentUserModel | undefined = undefined;

  render() {
    return html`
        <uui-box headline="Who am I? [Server]">
            <h2>${this._serverUserData?.email ? this._serverUserData.email : 'Dunno'}</h2>
            <ul>
                ${this._serverUserData?.groups.map(group => html`<li>${group.name}</li>`)}
            </ul>
            <uui-button color="default" look="primary" @click="${this.#onClickWhoAmI}">
                Who am I?
            </uui-button>
        </uui-box>

        <uui-box headline="Whats the time?">
            <h2><uui-icon name="icon-alarm-clock"></uui-icon> ${this._timeFromMrWolf ? this._timeFromMrWolf.toLocaleString() : 'Dunno'}</h2>
            <uui-button color="default" look="primary" @click="${this.#onClickWhatsTheTimeMrWolf}">
                Whats the time Mr Wolf?
            </uui-button>
        </uui-box>

        <uui-box headline="What's my name again?">
            <h2><uui-icon name="icon-user"></uui-icon> ${this._yourName}</h2>
            <uui-button color="default" look="primary" @click="${this.#onClickWhatsMyName}">
                Whats my name again?
            </uui-button>
        </uui-box>

        <uui-box headline="Who am I? [Context]" class="wide">
          <h2>${this._contextCurrentUser?.email}</h2>
          <umb-code-block language="json" copy>${JSON.stringify(this._contextCurrentUser, null, 2)}</umb-code-block>
        </uui-box>
    `;
  }

  static styles = [
    css`
            :host {
                display: grid;
                gap: var(--uui-size-layout-1);
                padding: var(--uui-size-layout-1);
                grid-template-columns: 1fr 1fr 1fr;
            }

            uui-box {
                margin-bottom: var(--uui-size-layout-1);
            }

            h2 {
                margin-top:0;
            }

            .wide {
                grid-column: span 3;
            }
    `];
}

export default MyAwesomeDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    'my-awesome-dashboard': MyAwesomeDashboardElement;
  }
}
