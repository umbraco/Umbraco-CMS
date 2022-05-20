import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

@defineElement('umb-media-section')
export class UmbMediaSection extends LitElement {
  static styles = [
    UUITextStyles,
    css``,
  ];

  render() {
    return html`<div>Media Section</div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-media-section': UmbMediaSection;
  }
}