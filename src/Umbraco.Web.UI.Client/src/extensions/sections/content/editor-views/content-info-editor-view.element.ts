import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-content-info-editor-view')
export class UmbContentInfoEditorView extends LitElement {
  static styles = [
    UUITextStyles,
    css``,
  ];

  @property()
  node: any;

  render() {
    return html`<div>Info Editor View</div>`;
  }
}

export default UmbContentInfoEditorView;

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-info-editor-view': UmbContentInfoEditorView;
  }
}
