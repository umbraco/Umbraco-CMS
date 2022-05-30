import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

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

  @property()
  id!: string;


  private _onPropertyDataTypeChange(e: CustomEvent) {

    const target = (e.target as any)
    console.log(target.value)

    // TODO: Set value.
    //this.nodeData.properties.find(x => x.propertyAlias === target.propertyAlias)?.tempValue = target.value;
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

  /*
  // Properties mock data:
  private properties = [
    {
      propertyAlias: 'myHeadline',
      label: 'Text string label',
      description: 'This is the a text string property',
      dataTypeAlias: 'myTextStringEditor'
    },
    {
      propertyAlias: 'myDescription',
      label: 'Textarea label',
      description: 'this is a textarea property',
      dataTypeAlias: 'myTextAreaEditor'
    }
  ];
  */

  private nodeData = {
    name: 'my node 1',
    key: '1234-1234-1234',
    alias: 'myNode1',
    documentTypeAlias: 'myDocumentType',
    documentTypeKey: '1234-1234-1234',
    /* example of layout:
    layout: [
      {
        type: 'group',
        children: [
          {
            type: 'property',
            alias: 'myHeadline'
          },
          {
            type: 'property',
            alias: 'myDescription'
          }
        ]
      }
    ],
    */
    properties: [
      {
        alias: 'myHeadline',
        label: 'Textarea label',
        description: 'this is a textarea property',
        dataTypeAlias: 'myTextStringEditor',
        tempValue: 'hello world'
      },
      {
        alias: 'myDescription',
        label: 'Text string label',
        description: 'This is the a text string property',
        dataTypeAlias: 'myTextAreaEditor',
        tempValue: 'Tex areaaaa'
      },
    ],
    data: [
      {
        alias: 'myHeadline',
        value: 'hello world',
      },
      {
        alias: 'myDescription',
        value: 'Teeeeexxxt areaaaaaa',
      },
    ]
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
          <h1 style="margin-bottom: 40px;">RENDER NODE WITH ID: ${this.id}</h1>
          <!-- TODO: Make sure map get data from data object?, parse on property object. -->
          ${this.nodeData.properties.map(
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
