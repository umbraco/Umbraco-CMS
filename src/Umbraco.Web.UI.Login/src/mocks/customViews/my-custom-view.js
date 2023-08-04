const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host {
      display: block;
    }
  </style>
  <div>
    <uui-button id="button" look="primary" label="My custom button">
        <uui-icon name="favorite"></uui-icon>
        My Custom button <span id="providerName"></span>
    </uui-button>
  </div>
`;

export class MyCustomView extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.shadowRoot.appendChild(template.content.cloneNode(true));

    this.shadowRoot.getElementById('button').addEventListener('click', () => {
      alert('My custom button clicked');
    });
  }

  connectedCallback() {
    console.log('My custom view connected');
    this.shadowRoot.getElementById('providerName').innerText = this.getAttribute('provider-name');
  }
}

customElements.define('my-custom-view', MyCustomView);

export default MyCustomView;
