import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';

import './content-tree.element';

@customElement('umb-content-section')
export class UmbContentSection extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host,
      #router-slot {
        display: flex;
        width: 100%;
        height: 100%;
      }
    `,
  ];

  @state()
  private _routes: Array<IRoute> = [
    {
      path: 'dashboard',
      component: () => import('../../../section/section-dashboards.element'),
    },
    {
      path: 'node/:nodeId',
      component: () => import('./content-editor.element'),
      setup: (component: HTMLElement, info: IRoutingInfo) => {
        this._currentNodeId = info.match.params.nodeId;
        component.id = this._currentNodeId;
      },
    },
    {
      path: '**',
      redirectTo: 'dashboard',
    },
  ];

  @state()
  private _currentNodeId!: string;

  render() {
    return html`
      <!-- TODO: Figure out how we name layout components -->
      <umb-section-sidebar>
        <umb-content-tree .id="${this._currentNodeId}"></umb-content-tree>
      </umb-section-sidebar>
      <router-slot id="router-slot" .routes="${this._routes}"></router-slot>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-section': UmbContentSection;
  }
}
