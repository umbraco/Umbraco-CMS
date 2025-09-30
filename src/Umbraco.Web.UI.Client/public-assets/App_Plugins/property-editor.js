const template = document.createElement('template');
template.innerHTML = `  
  <span>Example of a vanilla JS Property Editor</span>
`;

export default class MyPropertyEditorUI extends HTMLElement {
	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		this.shadowRoot.appendChild(template.content.cloneNode(true));
	}
}
customElements.define('my-property-editor-ui-custom', MyPropertyEditorUI);
