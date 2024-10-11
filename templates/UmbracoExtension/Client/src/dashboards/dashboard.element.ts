import { LitElement, css, html, customElement, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { ExamplesService, UserModel } from "../api";
import { UUIButtonElement } from "@umbraco-cms/backoffice/external/uui";
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from "@umbraco-cms/backoffice/notification";

@customElement('my-awesome-dashboard')
export class MyAwesomeDashboardElement extends UmbElementMixin(LitElement) {

  constructor() {
    super();

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
      this.#notificationContext = notificationContext;
    });
  }

  #notificationContext: UmbNotificationContext | undefined = undefined;

  #whoAmI = async (ev: Event) => {
    const buttonElement = ev.target as UUIButtonElement;
    buttonElement.state = "waiting";

    const { data, error } = await ExamplesService.getUmbracoExampleApiV1WhoAmI();

    if (error) {
      buttonElement.state = "failed";
      console.error(error);
      return;
    }

    if (data !== undefined) {
      this.userData = data;
      buttonElement.state = "success";
    }

    if (this.#notificationContext) {
      this.#notificationContext.peek("warning", {
        data: {
          headline: `You are ${this.userData?.name}`,
          message: `Your email is ${this.userData?.email}`,
        }
      })
    }
  }

  #whatsTheTimeMrWolf = async (ev: Event) => {
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
      this.timeFromMrWolf = new Date(data);
      buttonElement.state = "success";
    }
  }

  #whatsMyName = async (ev: Event) => {
    const buttonElement = ev.target as UUIButtonElement;
    buttonElement.state = "waiting";

    const { data, error } = await ExamplesService.getUmbracoExampleApiV1WhatsMyName();

    if (error) {
      buttonElement.state = "failed";
      console.error(error);
      return;
    }

    this.yourName = data;
    buttonElement.state = "success";
  }

  @property()
  yourName: string | undefined = "Dunno";

  @property({ attribute: false })
  timeFromMrWolf: Date | undefined;

  @property({ type: Object })
  userData: UserModel | undefined = undefined;

  render() {
    return html`
        <uui-box headline="Who am I ?">
            <h2>${this.userData?.email ? this.userData.email : 'Dunno'}</h2>
            <ul>
                ${this.userData?.allowedSections.map(section => html`<li>${section}</li>`)}
            </ul>
            <uui-button color="positive" look="primary" @click="${this.#whoAmI}">
                <uui-icon name="icon-user"></uui-icon>
                Who am I?
            </uui-button>
        </uui-box>

        <uui-box headline="Whats the time?">
            <h2><uui-icon name="icon-alarm-clock"></uui-icon> ${this.timeFromMrWolf ? this.timeFromMrWolf.toLocaleString() : 'Dunno'}</h2>
            <uui-button color="positive" look="primary" @click="${this.#whatsTheTimeMrWolf}">
                <uui-icon name="icon-time"></uui-icon>
                Whats the time Mr Wolf?
            </uui-button>
        </uui-box>

        <uui-box headline="What's my name again?">
            <h2><uui-icon name="icon-user"></uui-icon> ${this.yourName}</h2>
            <uui-button color="positive" look="primary" @click="${this.#whatsMyName}">
                <uui-icon name="icon-help-alt"></uui-icon>
                Whats my name again?
            </uui-button>
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
    `];
}

export default MyAwesomeDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    'my-awesome-dashboard': MyAwesomeDashboardElement;
  }
}
