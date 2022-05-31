import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../core/context';
import { UmbRouteLocation, UmbRouter } from '../core/router';
import { UmbNodesStore } from '../core/stores/nodes.store';
import { Subscription } from 'rxjs';

import './content-tree.element';
import './content-dashboards.element';
import './content-editor.element';

@customElement('umb-content-section')
export class UmbContentSection extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: flex;
        width: 100%;
        height: 100%;
      }
    `,
  ];

  private _router?: UmbRouter;
  private _locationSubscription?: Subscription;
  private _outlet?: HTMLElement;

  constructor () {
    super();

    this.provideContext('umbContentService', new UmbNodesStore());

    this.consumeContext('umbRouter', (_instance: UmbRouter) => {
      this._router = _instance;
      this._useLocation();
    });
  }

  private _useLocation () {
    this._locationSubscription?.unsubscribe();

    this._locationSubscription = this._router?.location
    .subscribe((location: UmbRouteLocation) => {
      // TODO: temp outlet solution
      const nodeId = location.params.nodeId;

      if (nodeId !== undefined) {
        const contentEditor = document.createElement('umb-content-editor');
        contentEditor.id = nodeId;
        this._outlet = contentEditor;
        this.requestUpdate();
        return;
      }

      const dashboards = document.createElement('umb-content-dashboards');
      this._outlet = dashboards;
      this.requestUpdate();      
    });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._locationSubscription?.unsubscribe();
  }

  render() {
    return html`
      <!-- TODO: Figure out how we name layout components -->
      <umb-backoffice-sidebar>
        <umb-content-tree></umb-content-tree>
      </umb-backoffice-sidebar>
      ${this._outlet}
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-section': UmbContentSection;
  }
}
