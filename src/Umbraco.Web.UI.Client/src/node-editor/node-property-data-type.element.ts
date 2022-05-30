import { css, html, LitElement, PropertyValueMap } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';

// TODO: get from Data Type Service?
// TODO: do not have elementName, instead use extension-alias(property editor UI alias) to retrieve element Name, and ensure loaded JS resource.
const DataTypeInGlobalService = [
  {
    alias: 'myTextStringEditor',
    /*
    TODO: use this instead, look up extension API.
    propertyEditorUIAlias: 'Umb.PropertyEditorUI.TextString',
    */
    elementName: 'umb-property-editor-text'
  },
  {
    alias: 'myTextAreaEditor',
    elementName: 'umb-property-editor-textarea'
  }
];

@customElement('umb-node-property-data-type')
class UmbNodePropertyDataType extends LitElement {
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
    const oldValue = this._dataTypeAlias
    this._dataTypeAlias = alias;

    const found = DataTypeInGlobalService.find(x => x.alias === alias);
    if(!found) {
      // TODO: did not find data-type..
      // TODO: Consider error if undefined, showing a error-data-type, if super duper admin we might show a good error message(as always) and a editable textarea with the value, so there is some debug option available?
      return;
    }
    this._element = document.createElement(found.elementName);
    // TODO: Set/Parse Data-Type-UI-configuration
    this._element.addEventListener('property-editor-change', this._onPropertyEditorChange);

    this._element.value = this.value;// Be aware its duplicated code
    this.requestUpdate('element', oldValue);
  }
  

  // TODO: make interface for UMBPropertyEditorElement
  @state()
  private _element?:any;

  @property()
  value?:string;

  private _onPropertyEditorChange = ( e:CustomEvent) => {
    if(e.currentTarget === this._element) {
      this.value = this._element.value;
      // 
      this.dispatchEvent(new CustomEvent('property-data-type-change', { bubbles: true, composed: true }));
    }
    // make sure no event leave this scope.
    e.stopPropagation();
  }

  /** Lit does not currently handle dynamic tag names, therefor we are doing some manual rendering */
  // TODO: Refactor into a base class for dynamic-tag element? we will be using this a lot for extensions.
  // This could potentially hook into Lit and parse all properties defined in the specific class on to the dynamic-element. (see static elementProperties: PropertyDeclarationMap;)
  willUpdate(changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
    super.willUpdate(changedProperties);
    
    const hasChangedProps = changedProperties.has('value');
    if(hasChangedProps && this._element) {
      this._element.value = this.value;// Be aware its duplicated code
    }
  }


  render() {
    return html`${this._element}`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-node-property-control': UmbNodePropertyDataType;
  }
}
