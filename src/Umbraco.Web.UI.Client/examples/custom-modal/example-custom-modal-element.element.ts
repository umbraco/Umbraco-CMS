import { css, html } from "@umbraco-cms/backoffice/external/lit";
import { defineElement, UUIModalElement } from "@umbraco-cms/backoffice/external/uui";

/**
 * This class defines a custom design for the modal it self, in the same was as
 * UUIModalSidebarElement and UUIModalDialogElement.
 */
@defineElement('example-modal-element')
export class UmbExampleCustomModalElement extends UUIModalElement {
  override render() {
    return html`
      <dialog>
				<h2>Custom Modal-wrapper</h2>
				<slot></slot>
      </dialog>
    `;
  }

  static override styles = [
    ...UUIModalElement.styles,
    css`
      dialog {
        width:100%;
        height:100%;
        max-width: 100%;
        max-height: 100%;
				top:0;
				left:0;
				right:0;
				bottom:0;
				background:#fff;
      }
      :host([index='0']) dialog {
        box-shadow: var(--uui-shadow-depth-5);
      }
      :host(:not([index='0'])) dialog {
        outline: 1px solid rgba(0, 0, 0, 0.1);
      }

    `,
  ];
}

export default UmbExampleCustomModalElement;

declare global {
  interface HTMLElementTagNameMap {
    'example-modal-element': UmbExampleCustomModalElement;
  }
}
