import { html, css, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { worker } from './mocks/browser';

/**
 * An example element.
 *
 * @slot - This element has a slot
 * @csspart button - The button
 */
@customElement('my-element')
export class MyElement extends LitElement {
  static styles = css`
    :host {
      display: block;
      border: solid 1px gray;
      padding: 16px;
      max-width: 800px;
    }
  `;

  /**
   * The name to say "Hello" to.
   */
  @property()
  name = 'World';

  /**
   * The number of times the button has been clicked.
   */
  @property({ type: Number })
  count = 0;

  @state()
  _authorized = false;

  @state()
  _user: any;

  constructor() {
    super();
    worker.start();
  }

  private async _onLogin() {
    try {
      await fetch('/login', { method: 'POST' });
      this._authorized = true;
      this._getUser();
    } catch (error) {
      console.log(error);
    }
  }

  private _onLogout() {
    try {
      fetch('/logout', { method: 'POST' });
      this._authorized = false;
    } catch (error) {
      console.log(error);
    }
  }

  private async _getUser() {
    try {
      const res = await fetch('/user');
      this._user = await res.json();
    } catch (error) {
      console.log(error);
    }
  }

  render() {
    return html`
      <h1>Hello, ${this.name}!</h1>
      <button @click=${this._onClick} part="button">Click Count: ${this.count}</button>

      <h2>Login Here</h2>
      ${this._authorized
        ? html`<button @click=${this._onLogout}>Logout</button>`
        : html`<button @click=${this._onLogin}>Login</button>`}
      ${this._authorized && this._user
        ? html`
            <h3>User information</h3>
            <div>Username: ${this._user.username}</div>
          `
        : ''}

      <slot></slot>
    `;
  }

  private _onClick() {
    this.count++;
    fetch('/login', { method: 'POST' });
  }

  foo(): string {
    return 'foo';
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'my-element': MyElement;
  }
}
