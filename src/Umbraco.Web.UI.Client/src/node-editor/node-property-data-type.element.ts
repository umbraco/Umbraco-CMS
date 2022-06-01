import { css, html, LitElement, PropertyValueMap } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../core/context';
import { UmbDataTypeStore } from '../core/stores/data-type.store';
import { Subscription, map, switchMap } from 'rxjs';
import { DataTypeEntity } from '../mocks/data/content.data';
import { UmbExtensionManifest, UmbExtensionRegistry } from '../core/extension';
import { createExtensionElement } from '../core/extension/create-extension-element.function';

@customElement('umb-node-property-data-type')
class UmbNodePropertyDataType extends UmbContextConsumerMixin(LitElement) {
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
  private _dataTypeKey?: string | undefined;
  public get dataTypeKey(): string | undefined {
    return this._dataTypeKey;
  }
  public set dataTypeKey(key: string | undefined) {
    //const oldValue = this._dataTypeKey;
    this._dataTypeKey = key;
    this._useDataType();
  }

  // TODO: make interface for UMBPropertyEditorElement
  @state()
  private _element?: {value?:string} & HTMLElement;// TODO: invent interface for propertyEditorUI.

  @property()
  value?: string;

  @state()
  _dataType?: DataTypeEntity;

  @state()
  _propertyEditorUI?: UmbExtensionManifest;

  private _extensionRegistry?: UmbExtensionRegistry;
  private _dataTypeStore?: UmbDataTypeStore;
  private _dataTypeSubscription?: Subscription;

  constructor() {
    super();
    this.consumeContext('umbDataTypeStore', (_instance: UmbDataTypeStore) => {
      this._dataTypeStore = _instance;
      this._useDataType();
    });
    this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
      this._extensionRegistry = _instance;
      this._useDataType();
    });

    // TODO: solution to know when both contexts are available
  }

  // TODO: use subscribtion, rename to _useDataType:
  private _useDataType() {
    this._dataTypeSubscription?.unsubscribe();
    if (this._dataTypeKey && this._extensionRegistry && this._dataTypeStore) {
      //this._dataTypeSubscription = this._dataTypeStore.getByKey(this._dataTypeKey).subscribe(this._gotDataType);

      this._dataTypeSubscription = this._dataTypeStore
        .getByKey(this._dataTypeKey)
        .pipe(
          // eslint-disable-next-line @typescript-eslint/ban-ts-comment
          // @ts-ignore
          map((dataTypeEntity: DataTypeEntity) => {
            this._dataType = dataTypeEntity;
            return dataTypeEntity.propertyEditorUIAlias;
          }),
          switchMap((alias: string) => this._extensionRegistry?.getByAlias(alias) as any)
        )
        .subscribe((propertyEditorUI: any) => {
          this._propertyEditorUI = propertyEditorUI;
          this._gotData(this._dataType, this._propertyEditorUI);
        });
    }
  }

  private _gotData(_data?: DataTypeEntity, _propertyEditorUI?: UmbExtensionManifest) {
    if (!_data || !_propertyEditorUI) {
      // TODO: if dataTypeKey didn't exist in store, we should do some nice UI.
      return;
    }


    createExtensionElement(_propertyEditorUI).then(el => {

      const oldValue = this._element;
      this._element = el;
  
      // TODO: Set/Parse Data-Type-UI-configuration
  
      if(oldValue) {
        oldValue.removeEventListener('property-editor-change', this._onPropertyEditorChange as any as EventListener);
      }

      if(this._element) {
        this._element.addEventListener('property-editor-change', this._onPropertyEditorChange as any as EventListener);
        this._element.value = this.value;// Be aware its duplicated code
      }
      this.requestUpdate('element', oldValue);
    }).catch(() => {
      // TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
    });

  }

  private _onPropertyEditorChange = (e: CustomEvent) => {
    if (e.currentTarget === this._element) {
      this.value = this._element.value;
      //
      this.dispatchEvent(new CustomEvent('property-data-type-change', { bubbles: true, composed: true }));
    }
    // make sure no event leave this scope.
    e.stopPropagation();
  };

  /** Lit does not currently handle dynamic tag names, therefor we are doing some manual rendering */
  // TODO: Refactor into a base class for dynamic-tag element? we will be using this a lot for extensions.
  // This could potentially hook into Lit and parse all properties defined in the specific class on to the dynamic-element. (see static elementProperties: PropertyDeclarationMap;)
  willUpdate(changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
    super.willUpdate(changedProperties);

    const hasChangedProps = changedProperties.has('value');
    if (hasChangedProps && this._element) {
      this._element.value = this.value; // Be aware its duplicated code
    }
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._dataTypeSubscription?.unsubscribe();
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
