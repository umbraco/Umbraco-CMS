import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';

import './content-section-tree.element';

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
      component: () => import('../../components/section-dashboards.element'),
      setup: () => {
        this._currentNodeId = undefined;
      },
    },
    {
      path: 'node/:nodeId',
      component: () => import('../../components/node-editor.element'),
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
  private _currentNodeId?: string;

  render() {
    return html`
      <umb-section-sidebar>
        <umb-content-section-tree .currentNodeId="${this._currentNodeId}"></umb-content-section-tree>
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
