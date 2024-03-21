import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

@customElement('my-welcome-dashboard')
export default class MyWelcomeDashboard extends UmbElementMixin(LitElement) {
  #notificationContext?: typeof UMB_NOTIFICATION_CONTEXT_TOKEN.TYPE;

  @property() private hitCount: number = 0;
  @property() private buttonColor: string = 'positive';
  @property() private icon: string = 'umb:handtool';
  @property() private disabled: boolean = false

  constructor() {
    super();
    this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (_instance) => {
      this.#notificationContext = _instance;
    });
  }

  #onClick = () => {
    if (this.hitCount > 11) {
      this.disabled = true;
    } else if (this.hitCount > 8) {
      this.#notificationContext?.peek('danger', { data: { message: 'Please stop' } });
      this.buttonColor = 'danger';
      this.icon = 'umb:alert';
    } else if (this.hitCount > 4) {
      this.#notificationContext?.peek('warning', { data: { message: 'Okay, that\'s enough' } });
      this.buttonColor = 'warning';
    } else {
      this.#notificationContext?.peek('positive', { data: { message: '#h5yr' } });
    }
    this.hitCount++;
  }

  render() {
    return html`
			<uui-box headline="Welcome">
				<p>A TypeScript Lit Dashboard</p>
				<uui-button ?disabled=${this.disabled} style="font-size:2rem;" look="primary" color=${this.buttonColor} label="Click me" @click=${this.#onClick} compact><uui-icon name=${this.icon}></uui-icon></uui-button>
			</uui-box>
		`;
  }

  static styles = [
      css`
        :host {
            display: block;
            padding: var(--uui-size-layout-1);
        }
      `
  ]
}

declare global {
  interface HTMLElementTagNameMap {
    'my-welcome-dashboard': MyWelcomeDashboard;
  }
}