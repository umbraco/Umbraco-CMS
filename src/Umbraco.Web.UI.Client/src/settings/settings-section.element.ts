import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../core/context';
import { UmbExtensionManifest, UmbExtensionRegistry } from '../core/extension';

@customElement('umb-settings-section')
export class UmbSettingsSection extends UmbContextConsumerMixin(LitElement) {
  @state()
  private _extensions: Array<UmbExtensionManifest<any>> = [];

  private _extensionRegistry?: UmbExtensionRegistry;
  private _extensionsSubscription?: Subscription;

  constructor () {
    super();

    this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
      this._extensionRegistry = _instance;

      this._extensionsSubscription?.unsubscribe();

      this._extensionsSubscription = this._extensionRegistry.extensions.subscribe(extensions => {
        this._extensions = [...extensions];
      });
    });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._extensionsSubscription?.unsubscribe();
  }

  render() {
    return html`
      <uui-box headline="Extensions">
        <uui-table>
          <uui-table-head>
            <uui-table-head-cell>Type</uui-table-head-cell>
            <uui-table-head-cell>Name</uui-table-head-cell>
            <uui-table-head-cell>Alias</uui-table-head-cell>
          </uui-table-head>

          ${ this._extensions.map(extension => html`
            <uui-table-row>
              <uui-table-cell>${ extension.type }</uui-table-cell>
              <uui-table-cell>${ extension.name }</uui-table-cell>
              <uui-table-cell>${ extension.alias }</uui-table-cell>
            </uui-table-row>
          `)}
        </uui-table>
      </uui-box>
    `
  }
}