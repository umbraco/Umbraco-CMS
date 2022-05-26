import { css, LitElement, PropertyValueMap } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

// TODO: get from Data Type Service?
const DataTypeInGlobalService = [
  {
    alias: 'myTextStringEditor',
    elementName: 'umb-property-editor-text'
  },
  {
    alias: 'myTextAreaEditor',
    elementName: 'umb-property-editor-textarea'
  }
];

@customElement('umb-node-property-control')
class UmbNodePropertyControl extends LitElement {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: block;
        width: 100%;
        height: 100%;
      }
    `,
  ];

  @property()
  private _dataTypeAlias?: string | undefined;
  public get dataTypeAlias(): string | undefined {
    return this._dataTypeAlias;
  }
  public set dataTypeAlias(alias: string | undefined) {
    //const oldValue = this._dataTypeAlias
    this._dataTypeAlias = alias;
    const found = DataTypeInGlobalService.find(x => x.alias === alias);
    this.elementName = found?.elementName || undefined;
    // TODO: Consider error if undefined, showing a error-data-type, if super duper admin we might show a good error message(as always) and a editable textarea with the value, so there is some debug option available?
    //this.requestUpdate('dataTypeAlias', oldValue);
  }
  

  @property()
  elementName?:string

  @property()
  value?:string


  private _element?:HTMLElement;

  /** Lit does not currently handle dynamic tag names, therefor we are doing some manual rendering */
  // TODO: Refactor into a base class for dynamic-tag element? we will be using this a lot for extensions.
  // This could potentially hook into Lit and parse all properties defined in the specific class on to the dynamic-element. (see static elementProperties: PropertyDeclarationMap;)
  willUpdate(changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
    super.willUpdate(changedProperties);
    // only need to check changed properties for an expensive computation.
    const elementNameHasChanged = changedProperties.has('elementName');
    if (elementNameHasChanged) {

      if (this._element) {
        this.shadowRoot?.removeChild(this._element);
      }
      if(this.elementName) {
        this._element = document.createElement(this.elementName)
        this.shadowRoot?.appendChild(this._element);
      }
    }

    const hasChangedProps = changedProperties.has('value');
    if(hasChangedProps || elementNameHasChanged) {
      this._element?.setAttribute('value', this.value as any);
    }
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-node-property-control': UmbNodePropertyControl;
  }
}
