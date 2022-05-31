import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../core/context';
import { UmbNodeStore } from '../core/stores/node.store';
import { Subscription } from 'rxjs';
import { DocumentNode } from '../mocks/data/content.data';

@customElement('umb-content-editor')
class UmbContentEditor extends UmbContextConsumerMixin(LitElement) {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: block;
        width: 100%;
        height: 100%;
      }

      uui-input {
        width: 100%;
        margin-left: 16px;
      }

      uui-tab-group {
        --uui-tab-divider: var(--uui-color-border);
        border-left: 1px solid var(--uui-color-border);
        flex-wrap: nowrap;
        height: 60px;
      }

      uui-tab {
        font-size: 0.8rem;
      }

      uui-box hr {
        margin-bottom: var(--uui-size-6);
      }

      hr {
        border: 0;
        /* TODO: Use correct color property */
        border-top: 1px solid #e7e7e7;
      }
    `,
  ];

  @property()
  id!: string;

  @state()
  _node?: DocumentNode;

  private _contentService?: UmbNodeStore;
  private _nodeSubscription?: Subscription;

  constructor () {
    super();

    this.consumeContext('umbNodeStore', (contentService: UmbNodeStore) => {
      this._contentService = contentService;
      this._useNode();
    });
  }

  private _onPropertyDataTypeChange(e: CustomEvent) {
    const target = (e.target as any)
    console.log(target.value)

    // TODO: Set value.
    //this.nodeData.properties.find(x => x.propertyAlias === target.propertyAlias)?.tempValue = target.value;
  }

  private _useNode() {
    this._nodeSubscription?.unsubscribe();

    this._nodeSubscription = this._contentService?.getById(parseInt(this.id)).subscribe(node => {
      if (!node) return; // TODO: Handle nicely if there is no node.
      this._node = node;
    });
  }

  private _onSaveAndPublish() {
    console.log('Save and publish');
  }

  private _onSave() {
    console.log('Save');
  }

  private _onSaveAndPreview() {
    console.log('Save and preview');
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._nodeSubscription?.unsubscribe();
  }

  render() {
    return html`
      <umb-node-editor-layout>
        <uui-input slot="name" .value="${this._node?.name}"></uui-input>
        <uui-tab-group slot="apps">
          <uui-tab label="Content" active></uui-tab>
          <uui-tab label="Info"></uui-tab>
          <uui-tab label="Actions" disabled></uui-tab>
        </uui-tab-group>

        <uui-box slot="content">
          <!-- TODO: Make sure map get data from data object?, parse on property object. -->
          ${this._node?.properties.map(
            property => html`
            <umb-node-property 
              .property=${property}
              .value=${property.tempValue} 
              @property-data-type-change=${this._onPropertyDataTypeChange}>
            </umb-node-property>
            <hr />
          `)}
        </uui-box>

        <div slot="actions">
          <uui-button @click=${this._onSaveAndPreview} label="Save and preview"></uui-button>
          <uui-button @click=${this._onSave} look="secondary" label="Save"></uui-button>
          <uui-button @click=${this._onSaveAndPublish} look="primary" color="positive" label="Save and publish"></uui-button>
        </div>
      </umb-node-editor-layout>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-editor': UmbContentEditor;
  }
}
