import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

@defineElement('umb-content-section')
export class UmbContentSection extends LitElement {
  static styles = [
    UUITextStyles,
    css``,
  ];

  render() {
    return html`<div>Content Section</div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-section': UmbContentSection;
  }
}