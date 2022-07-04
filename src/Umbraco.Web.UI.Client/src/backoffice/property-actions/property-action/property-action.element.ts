import { UUITextStyles } from '@umbraco-ui/uui';
import { CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { createExtensionElement, UmbExtensionManifestPropertyAction } from '../../../core/extension';
import type { UmbPropertyAction } from './property-action.model';

@customElement('umb-property-action')
export class UmbPropertyActionElement extends LitElement implements UmbPropertyAction {
  static styles: CSSResultGroup = [
    UUITextStyles
  ];

  private _propertyAction?: UmbExtensionManifestPropertyAction;
  @property({ type: Object })
  public get propertyAction(): UmbExtensionManifestPropertyAction | undefined {
    return this._propertyAction;
  }
  public set propertyAction(value: UmbExtensionManifestPropertyAction | undefined) {
    this._propertyAction = value;
    this._createElement();
  }

  // TODO: we need to investigate context api vs values props and events
  @property()
  public value?: string;

  @state()
  private _element?: UmbPropertyActionElement;

  private async _createElement () {
    if (!this.propertyAction) return;

    try {
      this._element = await createExtensionElement(this.propertyAction) as UmbPropertyActionElement | undefined;
      if (!this._element) return;

      this._element.value = this.value;
    } catch (error) {
      // TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
    }
  }
  
  render () {
    return html`${this._element}`;
  }
}