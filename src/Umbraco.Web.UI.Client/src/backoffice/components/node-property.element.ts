import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, PropertyValueMap } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { EMPTY, of, Subscription, switchMap } from 'rxjs';

import { UmbContextConsumerMixin } from '../../core/context';
import { createExtensionElement, UmbExtensionManifest, UmbExtensionRegistry } from '../../core/extension';
import { UmbDataTypeStore } from '../../core/stores/data-type.store';
import { DataTypeEntity } from '../../mocks/data/content.data';

import '../property-actions/property-action-menu/property-action-menu.element';

@customElement('umb-node-property')
class UmbNodeProperty extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
			p {
				color: var(--uui-color-text-alt);
			}
		`,
	];

	private _property: any; // TODO: property data model interface..
	@property()
	public get property(): any {
		return this._property;
	}
	public set property(value: any) {
		this._property = value;
		this._useDataType();
	}

	@property()
	value?: string;

	// TODO: make interface for UMBPropertyEditorElement
	@state()
	private _element?: { value?: string } & HTMLElement; // TODO: invent interface for propertyEditorUI.

	private _dataType?: DataTypeEntity;
	private _extensionRegistry?: UmbExtensionRegistry;
	private _dataTypeStore?: UmbDataTypeStore;
	private _dataTypeSubscription?: Subscription;

	constructor() {
		super();

    /** TODO: Use DI for these types of services. */
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

  connectedCallback(): void {
    super.connectedCallback();
    this.addEventListener('property-editor-change', this._onPropertyEditorChange as any as EventListener);
  }

  // TODO: use subscribtion, rename to _useDataType:
  private _useDataType() {
    this._dataTypeSubscription?.unsubscribe();
    if (this._property.dataTypeKey && this._extensionRegistry && this._dataTypeStore) {
      this._dataTypeSubscription = this._dataTypeStore
        .getByKey(this._property.dataTypeKey)
        .pipe(
          switchMap((dataTypeEntity) => {
            if (!dataTypeEntity) {
              return EMPTY;
            }
            this._dataType = dataTypeEntity;

						return this._extensionRegistry?.getByAlias(dataTypeEntity.propertyEditorUIAlias) ?? of(null);
					})
				)
				.subscribe((propertyEditorUI) => {
					if (propertyEditorUI) {
						this._gotData(propertyEditorUI);
					}
					// TODO: If gone what then...
				});
		}
	}

	private _gotData(_propertyEditorUI?: UmbExtensionManifest) {
		if (!this._dataType || !_propertyEditorUI) {
			// TODO: if dataTypeKey didn't exist in store, we should do some nice UI.
			return;
		}

		createExtensionElement(_propertyEditorUI)
			.then((el) => {
				const oldValue = this._element;
				this._element = el;

        // TODO: Set/Parse Data-Type-UI-configuration
        if (this._element) {
          this._element.value = this.value; // Be aware its duplicated code
        }
        this.requestUpdate('element', oldValue);
      })
      .catch(() => {
        // TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
      });
  }

  private _onPropertyEditorChange = (e: CustomEvent) => {
    const target = e.composedPath()[0] as any;
    this.value = target.value;
    this.dispatchEvent(new CustomEvent('property-value-change', { bubbles: true, composed: true }));
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

  private _renderPropertyActionMenu () {
    return html`${ this._dataType ? html`<umb-property-action-menu .propertyEditorUIAlias="${this._dataType.propertyEditorUIAlias}" .value="${this.value}"></umb-property-action-menu>`: '' }`;
  }

  render() {
    return html`
      <umb-editor-property-layout>
        <div slot="header">
          <uui-label>${this.property.label}</uui-label>
          ${ this._renderPropertyActionMenu() }
          <p>${this.property.description}</p>
        </div>
        <div slot="editor">${this._element}</div>
      </umb-editor-property-layout>
    `;
  }
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-node-property': UmbNodeProperty;
	}
}
