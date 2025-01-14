const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host {
      padding: 20px;
      display: block;
      box-sizing: border-box;
    }
  </style>
  
  <uui-box>
    <h1>Custom Section</h1>
    <p>Example of vanilla JS section</p>
  </uui-box>
`;

export default class MySectionCustom extends HTMLElement {
	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		this.shadowRoot.appendChild(template.content.cloneNode(true));
	}
}

customElements.define('my-section-custom', MySectionCustom);
