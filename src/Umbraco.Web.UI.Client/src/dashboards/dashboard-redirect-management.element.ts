import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-redirect-management')
export class UmbDashboardRedirectManagement extends LitElement {
  static styles = [UUITextStyles, css``];

  render() {
    return html`<div>Redirect Management</div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-dashboard-redirect-management': UmbDashboardRedirectManagement;
  }
}
