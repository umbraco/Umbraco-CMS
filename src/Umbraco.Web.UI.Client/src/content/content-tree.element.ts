import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbContextConsumerMixin } from '../core/context';
import { UmbRouteLocation, UmbRouter } from '../core/router';
import { Subscription } from 'rxjs';
import { data } from '../mocks/data/content.data';
import { UUIMenuItemElement } from '@umbraco-ui/uui';

@customElement('umb-content-tree')
class UmbContentTree extends UmbContextConsumerMixin(LitElement) {

  static styles = [
    UUITextStyles,
    css`
      h3 {
        padding: var(--uui-size-4) var(--uui-size-8);
      }
    `,
  ];

  // simplified tree for testing
  @state()
  _tree: Array<any> = [];

  @state()
  _section?: string;

  @state()
  _currentNodeId?: number;

  private _router?: UmbRouter;
  private _location?: UmbRouteLocation;
  private _locationSubscription?: Subscription;

  constructor () {
    super();

    // TODO: implement correct tree data
    this._tree = data;

    this.consumeContext('umbRouter', (router: UmbRouter) => {
      this._router = router;
      this._useLocation();
    });
  }

  private _useLocation() {
    this._locationSubscription?.unsubscribe();

    this._locationSubscription = this._router?.location.subscribe(location => {
      this._location = location;
      this._section = location.params.section;
      this._currentNodeId = parseInt(location.params.nodeId);
    });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._locationSubscription?.unsubscribe();
  }

  /* TODO: there are some problems with menu items and click events. They can happen on element inside and outside of the shadow dom 
    which makes it difficult to find the right href in the router.
    It might make sense to make it possible to use your own anchor tag or button inside a label slot instead.
    This is a temp solution to get the href from the menu item and overwrite the router hijacking.
  */
  private _handleMenuItemClick (e: PointerEvent) {
    e.preventDefault();
    e.stopPropagation();

    const target = e.target as UUIMenuItemElement;
    if (!target) return;

    const href = target.href;
    if (!href) return;

    this._router?.push(href);
  }

  render () {
    return html`
      <a href="${`/section/${this._section}`}">
        <h3>Content</h3>
      </a>

      <div class="nav-list">
        <!-- TODO: make menu item events bubble so we don't have to attach event listeners on every item -->
        ${ this._tree.map(item => html`
          <uui-menu-item
            @click="${this._handleMenuItemClick}"
            ?active="${item.id === this._currentNodeId}"
            data-id="${item.id}"
            label="${item.name}"
            href="/section/${this._section}/node/${item.id}">
            <uui-icon slot="icon" name="${item.icon}"></uui-icon>
          </uui-menu-item>
        `)}
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-tree': UmbContentTree;
  }
}
