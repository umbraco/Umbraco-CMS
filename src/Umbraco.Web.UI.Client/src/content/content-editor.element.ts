import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';

import '../properties/node-property.element.ts';
import '../properties/property-editor-text.element.ts';
import '../properties/property-editor-textarea.element.ts';
import '../backoffice/node-editor-layout.element.ts';

@customElement('umb-content-editor')
class UmbContentEditor extends LitElement {
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

  private _onSaveAndPublish() {
    console.log('Save and publish');
  }

  private _onSave() {
    console.log('Save');
  }

  private _onSaveAndPreview() {
    console.log('Save and preview');
  }

  render() {
    return html`
      <umb-node-editor-layout>
        <uui-input slot="name" value="Home"></uui-input>
        <uui-tab-group slot="apps">
          <uui-tab active>Content</uui-tab>
          <uui-tab>Info</uui-tab>
          <uui-tab disabled>Actions</uui-tab>
        </uui-tab-group>

        <uui-box slot="content">
          <umb-node-property label="Text string label" description="This is the a text string property">
            <umb-property-editor-text></umb-property-editor-text>
          </umb-node-property>
          <hr />
          <umb-node-property label="Textarea label" description="this is a textarea property">
            <umb-property-editor-textarea></umb-property-editor-textarea>
          </umb-node-property>
        </uui-box>

        <div slot="actions">
          <uui-button @click=${this._onSaveAndPreview}>Save and preview</uui-button>
          <uui-button @click=${this._onSave} look="secondary">Save</uui-button>
          <uui-button @click=${this._onSaveAndPublish} look="primary" color="positive">Save and publish</uui-button>
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
