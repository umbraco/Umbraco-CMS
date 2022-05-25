import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../core/context';
import { UmbExtensionManifest, UmbExtensionRegistry } from '../core/extension';

@customElement('umb-settings-section')
export class UmbSettingsSection extends UmbContextConsumerMixin(LitElement) {
  @state()
  private _extensions: Array<UmbExtensionManifest> = [];

  private _extensionRegistry?: UmbExtensionRegistry;
  private _extensionsSubscription?: Subscription;

  constructor () {
    super();

    this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
      this._extensionRegistry = _instance;

      // TODO: Could we make it easier to unsubscribe? If we invented a Pattern/Mixin/class ala Lit-Controllers we could make it auto unsubscribe.
      // ContextConsumers could be turned into single classes which uses the 'Controller' ability to hook into connected and disconnected.
      // Generally that means that a web component must have the ControllerMixin?? and then controllers can easily be attached, they would know about life cycle and thereby be able to unsubscribe on disconnected etc.
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